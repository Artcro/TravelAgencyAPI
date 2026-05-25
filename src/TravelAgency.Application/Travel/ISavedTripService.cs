using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface ISavedTripService
{
	Task<SavedTripResponse> SaveAsync(SaveTripRequest request, Guid? userId, CancellationToken cancellationToken);
	Task<IReadOnlyList<object>> ListAsync(Guid? userId, CancellationToken cancellationToken);
	Task<object?> GetByIdAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken);
	Task<bool> DeleteAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken);
}
