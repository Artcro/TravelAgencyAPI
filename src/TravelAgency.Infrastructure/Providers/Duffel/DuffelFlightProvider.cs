using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure.Providers.Duffel;

public sealed class DuffelFlightProvider(IHttpClientFactory factory, IOptions<DuffelOptions> options, TravelDbContext db, ILogger<DuffelFlightProvider> logger) : IFlightProvider
{
    private const int MaxErrorBodyChars = 4096;

    public async Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.AccessToken)) throw new InvalidOperationException("Duffel access token is not configured.");

        var payload = new DuffelOfferRequestEnvelope
        {
            Data = new DuffelOfferRequestData
            {
                Slices = BuildSlices(request),
                Passengers = BuildPassengers(request),
                CabinClass = MapCabinClass(request.TravelClass),
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? null : request.Currency
            }
        };

        var sw = Stopwatch.StartNew();
        var c = factory.CreateClient("duffel");
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.AccessToken);
        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        c.DefaultRequestHeaders.Add("Duffel-Version", options.Value.Version);

        var url = $"{options.Value.BaseUrl.TrimEnd('/')}/air/offer_requests";
        var bodyContent = JsonSerializer.Serialize(payload);
        var reqMessage = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(bodyContent, Encoding.UTF8, "application/json")
        };

        var res = await c.SendAsync(reqMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (res.Content.Headers.ContentLength is long contentLength && contentLength > options.Value.MaxResponseBytes)
        {
            await TrySaveProviderRequestLogAsync((int)res.StatusCode, false, $"Duffel response exceeded configured max response bytes ({contentLength} > {options.Value.MaxResponseBytes}).", sw.ElapsedMilliseconds, cancellationToken);
            throw new InvalidOperationException($"Duffel flights failed ({(int)res.StatusCode}): response too large ({contentLength} bytes).");
        }

        if (!res.IsSuccessStatusCode)
        {
            var errorBody = await ReadAndTruncateErrorBodyAsync(res, cancellationToken);
            await TrySaveProviderRequestLogAsync((int)res.StatusCode, false, errorBody, sw.ElapsedMilliseconds, cancellationToken);
            throw new InvalidOperationException($"Duffel flights failed ({(int)res.StatusCode}): {errorBody}");
        }

        await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
        var response = await JsonSerializer.DeserializeAsync<DuffelOfferResponseEnvelope>(stream, cancellationToken: cancellationToken);
        await TrySaveProviderRequestLogAsync((int)res.StatusCode, true, null, sw.ElapsedMilliseconds, cancellationToken);

        var offers = response?.Data?.Offers;
        if (offers is null || offers.Count == 0) return [];

        var results = new List<FlightOptionDto>(offers.Count);
        foreach (var offer in offers)
        {
            var outbound = GetSegments(offer, 0);
            var inbound = GetSegments(offer, 1);
            var firstSlice = offer.Slices is { Count: > 0 } ? offer.Slices[0] : null;

            _ = decimal.TryParse(offer.TotalAmount, out var amount);

            var airlineCode = offer.Owner?.IataCode ?? string.Empty;
            var airlineName = offer.Owner?.Name;

            results.Add(new FlightOptionDto
            {
                Provider = "Duffel",
                ProviderOfferId = offer.Id ?? string.Empty,
                AirlineCode = airlineCode,
                AirlineName = airlineName,
                TotalPrice = new MoneyDto(amount, offer.TotalCurrency ?? request.Currency),
                Duration = firstSlice?.Duration ?? string.Empty,
                Stops = Math.Max(0, outbound.Count - 1),
                OutboundSegments = outbound,
                ReturnSegments = inbound,
                DeepLink = null
            });
        }

        return results.Take(Math.Max(1, request.MaxFlightResults)).ToList();
    }

    private async Task TrySaveProviderRequestLogAsync(int statusCode, bool success, string? errorMessage, long durationMs, CancellationToken cancellationToken)
    {
        try
        {
            db.ProviderRequestLogs.Add(new ProviderRequestLogEntity { Id = Guid.NewGuid(), Provider = "Duffel", Endpoint = "/air/offer_requests", StatusCode = statusCode, Success = success, ErrorMessage = errorMessage, DurationMs = durationMs, CreatedAtUtc = DateTime.UtcNow });
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Best-effort provider request logging failed for Duffel. Continuing request processing.");
        }
    }

    private static async Task<string> ReadAndTruncateErrorBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body)) return "No error body returned by provider.";
        var compact = body.Replace('\n', ' ').Replace('\r', ' ').Trim();
        return compact.Length <= MaxErrorBodyChars ? compact : compact[..MaxErrorBodyChars] + "... [truncated]";
    }

    private static List<DuffelSliceRequest> BuildSlices(TripSearchRequest request)
    {
        var slices = new List<DuffelSliceRequest> { new() { Origin = request.Origin, Destination = request.Destination, DepartureDate = request.DepartureDate.ToString("yyyy-MM-dd") } };
        if (request.ReturnDate.HasValue) slices.Add(new DuffelSliceRequest { Origin = request.Destination, Destination = request.Origin, DepartureDate = request.ReturnDate.Value.ToString("yyyy-MM-dd") });
        return slices;
    }
    private static List<DuffelPassengerRequest> BuildPassengers(TripSearchRequest request)
    {
        var passengers = new List<DuffelPassengerRequest>();
        passengers.AddRange(Enumerable.Range(0, Math.Max(1, request.Adults)).Select(_ => new DuffelPassengerRequest { Type = "adult" }));
        passengers.AddRange(Enumerable.Range(0, Math.Max(0, request.Children)).Select(_ => new DuffelPassengerRequest { Type = "child" }));
        return passengers;
    }
    private static string? MapCabinClass(string travelClass) => travelClass?.Trim().ToUpperInvariant() switch { "ECONOMY" => "economy", "PREMIUM_ECONOMY" => "premium_economy", "BUSINESS" => "business", "FIRST" => "first", _ => null };
    private static List<TripSegmentDto> GetSegments(DuffelOffer offer, int index)
    {
        var result = new List<TripSegmentDto>();
        if (offer.Slices is null || offer.Slices.Count <= index) return result;
        var slice = offer.Slices[index];
        if (slice.Segments is null) return result;
        foreach (var seg in slice.Segments)
        {
            _ = DateTime.TryParse(seg.DepartingAt, out var depart);
            _ = DateTime.TryParse(seg.ArrivingAt, out var arrive);
            result.Add(new TripSegmentDto
            {
                Origin = seg.Origin?.IataCode ?? string.Empty,
                Destination = seg.Destination?.IataCode ?? string.Empty,
                DepartureAt = depart,
                ArrivalAt = arrive,
                CarrierCode = seg.MarketingCarrier?.IataCode ?? string.Empty,
                FlightNumber = seg.MarketingCarrierFlightNumber ?? string.Empty,
                Duration = seg.Duration ?? string.Empty
            });
        }
        return result;
    }
}
