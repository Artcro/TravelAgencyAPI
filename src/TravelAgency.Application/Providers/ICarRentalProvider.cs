using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Providers;

public interface ICarRentalProvider
{
	Task<IReadOnlyList<CarRentalOptionDto>> SearchCarsAsync(TripSearchRequest request,
		CancellationToken cancellationToken);
}
