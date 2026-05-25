using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Infrastructure.Services;

namespace TravelAgency.Infrastructure.Providers.Duffel;

public sealed class DuffelFlightProvider(
	IHttpClientFactory factory,
	IOptions<DuffelOptions> options,
	ProviderRequestLogger requestLogger,
	ILogger<DuffelFlightProvider> logger) : IFlightProvider
{
	private const int MaxErrorBodyChars = 4096;
	private const string ProviderName = "Duffel";
	private const string OfferRequestsEndpoint = "/air/offer_requests";

	public async Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request,
		CancellationToken cancellationToken)
	{
		var settings = options.Value;
		if (string.IsNullOrWhiteSpace(settings.AccessToken))
			throw new InvalidOperationException("Duffel access token is not configured.");

		var timeoutSeconds = settings.TimeoutSeconds > 0 ? settings.TimeoutSeconds : 45;
		var payload = new DuffelOfferRequestEnvelope
		{
			Data = new DuffelOfferRequestData
			{
				Slices = BuildSlices(request),
				Passengers = BuildPassengers(request),
				CabinClass = MapCabinClass(request.TravelClass),
				Currency = string.IsNullOrWhiteSpace(request.Currency) ? null : request.Currency,
				SupplierTimeout = settings.SupplierTimeoutMilliseconds > 0
					? settings.SupplierTimeoutMilliseconds
					: 15_000,
				MaxConnections = Math.Max(0, settings.MaxConnections),
				ReturnOffers = settings.UseReturnOffers
			}
		};

		var sw = Stopwatch.StartNew();
		var c = factory.CreateClient("duffel");
		c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);
		c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		c.DefaultRequestHeaders.Add("Duffel-Version", settings.Version);

		var url = $"{settings.BaseUrl.TrimEnd('/')}/air/offer_requests";
		var bodyContent = JsonSerializer.Serialize(payload);

		try
		{
			using var reqMessage = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = new StringContent(bodyContent, Encoding.UTF8, "application/json")
			};

			using var res = await c.SendAsync(reqMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

			if (res.Content.Headers.ContentLength is long contentLength && contentLength > settings.MaxResponseBytes)
			{
				await requestLogger.RecordAsync(ProviderName, OfferRequestsEndpoint, (int)res.StatusCode, false,
					sw.ElapsedMilliseconds,
					$"Duffel response exceeded configured max response bytes ({contentLength} > {settings.MaxResponseBytes}).",
					cancellationToken);

				throw new InvalidOperationException(
					$"Duffel flights failed ({(int)res.StatusCode}): response too large ({contentLength} bytes).");
			}

			if (!res.IsSuccessStatusCode)
			{
				var errorBody = await ReadAndTruncateErrorBodyAsync(res, cancellationToken);
				await requestLogger.RecordAsync(ProviderName, OfferRequestsEndpoint, (int)res.StatusCode, false,
					sw.ElapsedMilliseconds, errorBody, cancellationToken);

				throw new InvalidOperationException($"Duffel flights failed ({(int)res.StatusCode}): {errorBody}");
			}

			await using var stream = await res.Content.ReadAsStreamAsync(cancellationToken);
			var response =
				await JsonSerializer.DeserializeAsync<DuffelOfferResponseEnvelope>(stream,
					cancellationToken: cancellationToken);

			var offers = payload.Data.ReturnOffers ? response?.Data?.Offers : null;
			if (!payload.Data.ReturnOffers)
			{
				var offerRequestId = response?.Data?.Id;
				if (string.IsNullOrWhiteSpace(offerRequestId)) return [];

				var limit = Math.Max(1, settings.MaxOffersToRead);
				var offersUrl =
					$"{settings.BaseUrl.TrimEnd('/')}/air/offers?offer_request_id={Uri.EscapeDataString(offerRequestId)}&limit={limit}";

				using var offersRequest = new HttpRequestMessage(HttpMethod.Get, offersUrl);
				using var offersResponse = await c.SendAsync(offersRequest, HttpCompletionOption.ResponseHeadersRead,
					cancellationToken);

				if (!offersResponse.IsSuccessStatusCode)
				{
					var offersErrorBody = await ReadAndTruncateErrorBodyAsync(offersResponse, cancellationToken);
					await requestLogger.RecordAsync(ProviderName, OfferRequestsEndpoint,
						(int)offersResponse.StatusCode, false, sw.ElapsedMilliseconds, offersErrorBody,
						cancellationToken);

					throw new InvalidOperationException(
						$"Duffel flights failed ({(int)offersResponse.StatusCode}): {offersErrorBody}");
				}

				await using var offersStream = await offersResponse.Content.ReadAsStreamAsync(cancellationToken);
				var offersEnvelope =
					await JsonSerializer.DeserializeAsync<DuffelListOffersEnvelope>(offersStream,
						cancellationToken: cancellationToken);

				offers = offersEnvelope?.Data;
			}

			await requestLogger.RecordAsync(ProviderName, OfferRequestsEndpoint, (int)res.StatusCode, true,
				sw.ElapsedMilliseconds, cancellationToken: cancellationToken);

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

			return results.Take(Math.Min(Math.Max(1, request.MaxFlightResults), Math.Max(1, settings.MaxOffersToRead)))
				.ToList();
		}
		catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
		{
			var message = $"Duffel flight search timed out after {timeoutSeconds} seconds.";
			logger.LogWarning(ex, "{Message}", message);

			await requestLogger.RecordAsync(ProviderName, OfferRequestsEndpoint, 408, false, sw.ElapsedMilliseconds,
				message, CancellationToken.None);
			if (settings.ReturnEmptyOnTimeout) return [];

			throw new InvalidOperationException(message);
		}
	}

	private static async Task<string> ReadAndTruncateErrorBodyAsync(HttpResponseMessage response,
		CancellationToken cancellationToken)
	{
		var body = await response.Content.ReadAsStringAsync(cancellationToken);
		if (string.IsNullOrWhiteSpace(body)) return "No error body returned by provider.";
		var compact = body.Replace('\n', ' ').Replace('\r', ' ')
			.Replace("token", "[redacted]", StringComparison.OrdinalIgnoreCase).Trim();

		return compact.Length <= MaxErrorBodyChars ? compact : compact[..MaxErrorBodyChars] + "... [truncated]";
	}

	private static List<DuffelSliceRequest> BuildSlices(TripSearchRequest request)
	{
		var slices = new List<DuffelSliceRequest>
		{
			new()
			{
				Origin = request.Origin, Destination = request.Destination,
				DepartureDate = request.DepartureDate.ToString("yyyy-MM-dd")
			}
		};

		if (request.ReturnDate.HasValue)
			slices.Add(new DuffelSliceRequest
			{
				Origin = request.Destination, Destination = request.Origin,
				DepartureDate = request.ReturnDate.Value.ToString("yyyy-MM-dd")
			});

		return slices;
	}

	private static List<DuffelPassengerRequest> BuildPassengers(TripSearchRequest request)
	{
		var passengers = new List<DuffelPassengerRequest>();
		passengers.AddRange(Enumerable.Range(0, Math.Max(1, request.Adults))
			.Select(_ => new DuffelPassengerRequest { Type = "adult" }));

		passengers.AddRange(Enumerable.Range(0, Math.Max(0, request.Children))
			.Select(_ => new DuffelPassengerRequest { Type = "child" }));

		return passengers;
	}

	private static string? MapCabinClass(string travelClass)
	{
		return travelClass?.Trim().ToUpperInvariant() switch
		{
			"ECONOMY" => "economy", "PREMIUM_ECONOMY" => "premium_economy", "BUSINESS" => "business",
			"FIRST" => "first", _ => null
		};
	}

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
