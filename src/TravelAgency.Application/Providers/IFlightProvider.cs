using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Providers;

public interface IFlightProvider
{
	Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request,
		CancellationToken cancellationToken);
}
