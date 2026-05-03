using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure.Providers.Duffel;

public sealed class DuffelFlightProvider(IHttpClientFactory factory, IOptions<DuffelOptions> options, TravelDbContext db) : IFlightProvider
{
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
        var res = await c.PostAsync(url, new StringContent(bodyContent, Encoding.UTF8, "application/json"), cancellationToken);
        var body = await res.Content.ReadAsStringAsync(cancellationToken);

        db.ProviderRequestLogs.Add(new ProviderRequestLogEntity { Id = Guid.NewGuid(), Provider = "Duffel", Endpoint = "/air/offer_requests", StatusCode = (int)res.StatusCode, Success = res.IsSuccessStatusCode, DurationMs = sw.ElapsedMilliseconds, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync(cancellationToken);

        if (!res.IsSuccessStatusCode) throw new InvalidOperationException($"Duffel flights failed ({(int)res.StatusCode}).");

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("data", out var data)) return [];
        if (!data.TryGetProperty("offers", out var offers) || offers.ValueKind != JsonValueKind.Array) return [];

        var results = new List<FlightOptionDto>();
        foreach (var offer in offers.EnumerateArray())
        {
            var outbound = GetSegments(offer, 0);
            var inbound = GetSegments(offer, 1);
            var firstSlice = offer.TryGetProperty("slices", out var slices) && slices.ValueKind == JsonValueKind.Array && slices.GetArrayLength() > 0 ? slices[0] : default;
            var duration = firstSlice.ValueKind != JsonValueKind.Undefined && firstSlice.TryGetProperty("duration", out var d) ? d.GetString() ?? string.Empty : string.Empty;

            _ = decimal.TryParse(offer.TryGetProperty("total_amount", out var t) ? t.GetString() : "0", out var amount);
            var currency = offer.TryGetProperty("total_currency", out var ccy) ? ccy.GetString() ?? request.Currency : request.Currency;

            var airlineCode = offer.TryGetProperty("owner", out var owner) && owner.TryGetProperty("iata_code", out var iata) ? iata.GetString() ?? string.Empty : string.Empty;
            var airlineName = owner.ValueKind != JsonValueKind.Undefined && owner.TryGetProperty("name", out var name) ? name.GetString() : null;

            results.Add(new FlightOptionDto
            {
                Provider = "Duffel",
                ProviderOfferId = offer.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                AirlineCode = airlineCode,
                AirlineName = airlineName,
                TotalPrice = new MoneyDto(amount, currency),
                Duration = duration,
                Stops = Math.Max(0, outbound.Count - 1),
                OutboundSegments = outbound,
                ReturnSegments = inbound,
                DeepLink = null
            });
        }

        return results.Take(Math.Max(1, request.MaxFlightResults)).ToList();
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

    private static string? MapCabinClass(string travelClass) => travelClass?.Trim().ToUpperInvariant() switch
    {
        "ECONOMY" => "economy",
        "PREMIUM_ECONOMY" => "premium_economy",
        "BUSINESS" => "business",
        "FIRST" => "first",
        _ => null
    };

    private static List<TripSegmentDto> GetSegments(JsonElement offer, int index)
    {
        var result = new List<TripSegmentDto>();
        if (!offer.TryGetProperty("slices", out var slices) || slices.ValueKind != JsonValueKind.Array || slices.GetArrayLength() <= index) return result;
        var slice = slices[index];
        if (!slice.TryGetProperty("segments", out var segments) || segments.ValueKind != JsonValueKind.Array) return result;
        foreach (var seg in segments.EnumerateArray())
        {
            var depart = seg.TryGetProperty("departing_at", out var dep) && DateTime.TryParse(dep.GetString(), out var depAt) ? depAt : default;
            var arrive = seg.TryGetProperty("arriving_at", out var arr) && DateTime.TryParse(arr.GetString(), out var arrAt) ? arrAt : default;
            var marketing = seg.TryGetProperty("marketing_carrier", out var mc) ? mc : default;
            var opNum = seg.TryGetProperty("marketing_carrier_flight_number", out var fn) ? fn.GetString() ?? string.Empty : string.Empty;
            result.Add(new TripSegmentDto
            {
                Origin = seg.TryGetProperty("origin", out var o) && o.TryGetProperty("iata_code", out var oi) ? oi.GetString() ?? string.Empty : string.Empty,
                Destination = seg.TryGetProperty("destination", out var dst) && dst.TryGetProperty("iata_code", out var di) ? di.GetString() ?? string.Empty : string.Empty,
                DepartureAt = depart,
                ArrivalAt = arrive,
                CarrierCode = marketing.ValueKind != JsonValueKind.Undefined && marketing.TryGetProperty("iata_code", out var ci) ? ci.GetString() ?? string.Empty : string.Empty,
                FlightNumber = opNum,
                Duration = seg.TryGetProperty("duration", out var dur) ? dur.GetString() ?? string.Empty : string.Empty
            });
        }
        return result;
    }
}
