using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Providers;

public interface IActivityProvider
{
	Task<IReadOnlyList<ActivityOptionDto>> SearchActivitiesAsync(TripSearchRequest request,
		CancellationToken cancellationToken);
}
