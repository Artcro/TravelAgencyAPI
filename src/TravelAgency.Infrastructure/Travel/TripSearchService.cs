using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Domain.ValueObjects;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure.Travel;

public sealed class TripSearchService(
	TripSearchRequestValidator validator,
	TripResultNormalizer normalizer,
	IFlightProvider flightProvider,
	IHotelProvider hotelProvider,
	IActivityProvider activityProvider,
	TravelDbContext db,
	ILogger<TripSearchService> logger) : ITripSearchService
{
	public async Task<TripSearchResponse> SearchAsync(TripSearchRequest request, Guid? userId,
		CancellationToken cancellationToken)
	{
		var normalized = Normalize(request);
		var errors = validator.Validate(normalized);
		if (errors.Count > 0) throw new ArgumentException(string.Join(" ", errors));

		var warnings = new List<string>();
		IReadOnlyList<FlightOptionDto> flights;
		try
		{
			flights = await flightProvider.SearchFlightsAsync(normalized, cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Flight provider failed.");
			throw new InvalidOperationException("Flight provider failed.", ex);
		}

		var hotels = normalized.IncludeHotels
			? await TryProviderAsync("Hotel", warnings,
				() => hotelProvider.SearchHotelsAsync(normalized, cancellationToken))
			: [];

		var activities = normalized.IncludeActivities
			? await TryProviderAsync("Activity", warnings,
				() => activityProvider.SearchActivitiesAsync(normalized, cancellationToken))
			: [];

		var response = normalizer.Normalize(normalized, flights, hotels, activities, warnings);
		response.SearchId = Guid.NewGuid();

		await RecordSearchAsync(normalized, response, userId, warnings, cancellationToken);
		return response;
	}

	public async Task<TripSearchResponse?> GetSearchByIdAsync(Guid searchId, Guid? userId,
		CancellationToken cancellationToken)
	{
		var e = await db.TripSearches.FirstOrDefaultAsync(x => x.Id == searchId, cancellationToken);
		return e is null ? null : JsonSerializer.Deserialize<TripSearchResponse>(e.ResponseJson);
	}

	private static TripSearchRequest Normalize(TripSearchRequest request)
	{
		return new TripSearchRequest
		{
			Origin = request.Origin,
			Destination = request.Destination,
			DepartureDate = request.DepartureDate,
			ReturnDate = request.ReturnDate,
			Adults = request.Adults,
			Children = request.Children,
			Infants = request.Infants,
			Currency = Currency.Normalize(request.Currency),
			TravelClass = TravelClassParser.ToWire(TravelClassParser.Parse(request.TravelClass)),
			MaxFlightResults = request.MaxFlightResults,
			IncludeHotels = request.IncludeHotels,
			IncludeActivities = request.IncludeActivities
		};
	}

	private static async Task<List<T>> TryProviderAsync<T>(string label, List<string> warnings,
		Func<Task<IReadOnlyList<T>>> call)
	{
		try
		{
			var items = (await call()).ToList();
			warnings.Add($"{label} results are mocked.");
			return items;
		}
		catch
		{
			warnings.Add($"{label} provider unavailable.");
			return [];
		}
	}

	private async Task RecordSearchAsync(TripSearchRequest request, TripSearchResponse response, Guid? userId,
		IReadOnlyCollection<string> warnings, CancellationToken cancellationToken)
	{
		db.TripSearches.Add(new TripSearchEntity
		{
			Id = response.SearchId,
			UserId = userId,
			Origin = request.Origin,
			Destination = request.Destination,
			DepartureDate = request.DepartureDate,
			ReturnDate = request.ReturnDate,
			Adults = request.Adults,
			Children = request.Children,
			Infants = request.Infants,
			Currency = request.Currency,
			RequestJson = JsonSerializer.Serialize(request),
			ResponseJson = JsonSerializer.Serialize(response),
			CreatedAtUtc = DateTime.UtcNow,
			ProviderStatus = warnings.Any(x => x.Contains("unavailable")) ? "Partial" : "Completed"
		});

		await db.SaveChangesAsync(cancellationToken);
	}
}
