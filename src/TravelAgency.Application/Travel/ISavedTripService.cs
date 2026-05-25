using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface ISavedTripService
{
	Task<SavedTripResponse> SaveAsync(SaveTripRequest request, Guid? userId, CancellationToken cancellationToken);
	Task<IReadOnlyList<SavedTripSummaryDto>> ListAsync(Guid? userId, CancellationToken cancellationToken);
	Task<SavedTripDetailDto?> GetByIdAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken);
	Task<bool> DeleteAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken);
}
