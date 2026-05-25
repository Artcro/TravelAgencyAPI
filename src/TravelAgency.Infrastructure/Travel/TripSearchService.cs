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
		request.Currency = Currency.Normalize(request.Currency);
		request.TravelClass = TravelClassParser.ToWire(TravelClassParser.Parse(request.TravelClass));
		var errors = validator.Validate(request);
		if (errors.Count > 0) throw new ArgumentException(string.Join(" ", errors));

		var warnings = new List<string>();
		IReadOnlyList<FlightOptionDto> flights;
		try
		{
			flights = await flightProvider.SearchFlightsAsync(request, cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Flight provider failed.");
			throw new InvalidOperationException("Flight provider failed.", ex);
		}

		var hotels = new List<HotelOptionDto>();
		var acts = new List<ActivityOptionDto>();

		if (request.IncludeHotels)
			try
			{
				hotels = (await hotelProvider.SearchHotelsAsync(request, cancellationToken)).ToList();
				warnings.Add("Hotel results are mocked.");
			}
			catch
			{
				warnings.Add("Hotel provider unavailable.");
			}

		if (request.IncludeActivities)
			try
			{
				acts = (await activityProvider.SearchActivitiesAsync(request, cancellationToken)).ToList();
				warnings.Add("Activity results are mocked.");
			}
			catch
			{
				warnings.Add("Activity provider unavailable.");
			}

		var response = normalizer.Normalize(request, flights, hotels, acts, warnings);
		response.SearchId = Guid.NewGuid();

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
		return response;
	}

	public async Task<TripSearchResponse?> GetSearchByIdAsync(Guid searchId, Guid? userId,
		CancellationToken cancellationToken)
	{
		var e = await db.TripSearches.FirstOrDefaultAsync(x => x.Id == searchId, cancellationToken);
		return e is null ? null : JsonSerializer.Deserialize<TripSearchResponse>(e.ResponseJson);
	}
}
