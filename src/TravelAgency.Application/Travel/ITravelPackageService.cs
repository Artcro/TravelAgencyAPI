using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface ITravelPackageService
{
	Task<Result<TravelPackageDto>> SaveAsync(Guid userId, SaveTravelPackageRequest request,
		CancellationToken cancellationToken);

	Task<IReadOnlyList<TravelPackageDto>> ListAsync(Guid userId, CancellationToken cancellationToken);

	Task<TravelPackageDto?> CancelAsync(Guid userId, Guid packageId, CancellationToken cancellationToken);

	Task<bool> DeleteAsync(Guid userId, Guid packageId, CancellationToken cancellationToken);
}
