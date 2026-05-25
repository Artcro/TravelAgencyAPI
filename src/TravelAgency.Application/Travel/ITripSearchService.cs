using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface ITripSearchService
{
	Task<Result<TripSearchResponse>> SearchAsync(TripSearchRequest request, Guid? userId,
		CancellationToken cancellationToken);

	Task<TripSearchResponse?> GetSearchByIdAsync(Guid searchId, Guid? userId, CancellationToken cancellationToken);
}
