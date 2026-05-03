using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Providers;

public interface ILocationProvider
{
	Task<IReadOnlyList<LocationSuggestionDto>> SearchLocationsAsync(string query, CancellationToken cancellationToken);
}

public interface IFlightProvider
{
	Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request,
		CancellationToken cancellationToken);
}

public interface IHotelProvider
{
	Task<IReadOnlyList<HotelOptionDto>> SearchHotelsAsync(TripSearchRequest request,
		CancellationToken cancellationToken);
}

public interface IActivityProvider
{
	Task<IReadOnlyList<ActivityOptionDto>> SearchActivitiesAsync(TripSearchRequest request,
		CancellationToken cancellationToken);
}