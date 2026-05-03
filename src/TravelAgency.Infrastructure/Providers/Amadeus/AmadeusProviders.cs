using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Infrastructure.Providers.Amadeus;

public sealed class AmadeusLocationProvider(
	IHttpClientFactory factory,
	AmadeusAuthClient auth,
	IOptions<AmadeusOptions> options,
	TravelDbContext db,
	ILogger<AmadeusLocationProvider> logger) : ILocationProvider
{
	public async Task<IReadOnlyList<LocationSuggestionDto>> SearchLocationsAsync(string query,
		CancellationToken cancellationToken)
	{
		var sw = Stopwatch.StartNew();
		var c = factory.CreateClient("amadeus");
		c.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", await auth.GetAccessTokenAsync(cancellationToken));

		var url =
			$"{options.Value.BaseUrl}/v1/reference-data/locations?subType=CITY,AIRPORT&keyword={Uri.EscapeDataString(query)}";

		var res = await c.GetAsync(url, cancellationToken);
		var body = await res.Content.ReadAsStringAsync(cancellationToken);
		await TrySaveProviderRequestLogAsync("/v1/reference-data/locations", (int)res.StatusCode,
			res.IsSuccessStatusCode, sw.ElapsedMilliseconds, cancellationToken);

		if (!res.IsSuccessStatusCode) return [];

		using var doc = JsonDocument.Parse(body);
		if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) return [];
		return data.EnumerateArray().Select(x =>
		{
			var iata = x.TryGetProperty("iataCode", out var iataEl) ? iataEl.GetString() ?? "" : "";
			var name = x.TryGetProperty("name", out var nEl) ? nEl.GetString() ?? "" : "";
			var subtype = x.TryGetProperty("subType", out var sEl) ? sEl.GetString() ?? "" : "";
			var country = x.TryGetProperty("address", out var aEl) && aEl.TryGetProperty("countryCode", out var cEl)
				? cEl.GetString() ?? ""
				: "";

			return new LocationSuggestionDto(iata, name, subtype, country, $"{name} ({iata})");
		}).ToList();
	}

	private async Task TrySaveProviderRequestLogAsync(string endpoint, int statusCode, bool success, long durationMs,
		CancellationToken cancellationToken)
	{
		try
		{
			db.ProviderRequestLogs.Add(new ProviderRequestLogEntity
			{
				Id = Guid.NewGuid(), Provider = "Amadeus", Endpoint = endpoint, StatusCode = statusCode,
				Success = success, DurationMs = durationMs, CreatedAtUtc = DateTime.UtcNow
			});

			await db.SaveChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Best-effort provider request logging failed for Amadeus location provider.");
		}
	}
}

public sealed class AmadeusFlightProvider(
	IHttpClientFactory factory,
	AmadeusAuthClient auth,
	IOptions<AmadeusOptions> options,
	TravelDbContext db,
	ILogger<AmadeusFlightProvider> logger) : IFlightProvider
{
	public async Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request,
		CancellationToken cancellationToken)
	{
		var sw = Stopwatch.StartNew();
		var c = factory.CreateClient("amadeus");
		c.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", await auth.GetAccessTokenAsync(cancellationToken));

		var q =
			$"originLocationCode={request.Origin}&destinationLocationCode={request.Destination}&departureDate={request.DepartureDate:yyyy-MM-dd}&adults={request.Adults}&currencyCode={request.Currency}&max={request.MaxFlightResults}";

		if (request.ReturnDate is not null) q += $"&returnDate={request.ReturnDate:yyyy-MM-dd}";
		var res = await c.GetAsync($"{options.Value.BaseUrl}/v2/shopping/flight-offers?{q}", cancellationToken);
		var body = await res.Content.ReadAsStringAsync(cancellationToken);
		await TrySaveProviderRequestLogAsync("/v2/shopping/flight-offers", (int)res.StatusCode, res.IsSuccessStatusCode,
			sw.ElapsedMilliseconds, cancellationToken);

		if (!res.IsSuccessStatusCode) throw new InvalidOperationException("Amadeus flights failed.");

		using var doc = JsonDocument.Parse(body);
		if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
			throw new InvalidOperationException("Amadeus flights malformed response.");

		var results = new List<FlightOptionDto>();
		foreach (var o in data.EnumerateArray())
		{
			var offerId = o.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? string.Empty : string.Empty;
			var airlineCode =
				o.TryGetProperty("validatingAirlineCodes", out var va) && va.ValueKind == JsonValueKind.Array &&
				va.GetArrayLength() > 0
					? va[0].GetString() ?? string.Empty
					: string.Empty;

			var priceObj = o.TryGetProperty("price", out var pEl) ? pEl : default;
			var totalString =
				priceObj.ValueKind != JsonValueKind.Undefined && priceObj.TryGetProperty("total", out var tEl)
					? tEl.GetString()
					: null;

			_ = decimal.TryParse(totalString, out var total);
			var currency =
				priceObj.ValueKind != JsonValueKind.Undefined && priceObj.TryGetProperty("currency", out var cEl)
					? cEl.GetString() ?? request.Currency
					: request.Currency;

			var duration = string.Empty;
			var stops = 0;
			if (o.TryGetProperty("itineraries", out var itineraries) && itineraries.ValueKind == JsonValueKind.Array &&
			    itineraries.GetArrayLength() > 0)
			{
				var first = itineraries[0];
				duration = first.TryGetProperty("duration", out var dEl)
					? dEl.GetString() ?? string.Empty
					: string.Empty;

				stops = first.TryGetProperty("segments", out var segEl) && segEl.ValueKind == JsonValueKind.Array
					? Math.Max(0, segEl.GetArrayLength() - 1)
					: 0;
			}

			results.Add(new FlightOptionDto
			{
				Provider = "Amadeus",
				ProviderOfferId = offerId,
				AirlineCode = airlineCode,
				TotalPrice = new MoneyDto(total, currency),
				Duration = duration,
				Stops = stops
			});
		}

		return results;
	}

	private async Task TrySaveProviderRequestLogAsync(string endpoint, int statusCode, bool success, long durationMs,
		CancellationToken cancellationToken)
	{
		try
		{
			db.ProviderRequestLogs.Add(new ProviderRequestLogEntity
			{
				Id = Guid.NewGuid(), Provider = "Amadeus", Endpoint = endpoint, StatusCode = statusCode,
				Success = success, DurationMs = durationMs, CreatedAtUtc = DateTime.UtcNow
			});

			await db.SaveChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Best-effort provider request logging failed for Amadeus flight provider.");
		}
	}
}