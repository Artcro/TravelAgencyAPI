using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface IFrontendTravelTicketService
{
	Task<Result<IReadOnlyList<FrontendTravelTicketDto>>> SearchAsync(FrontendTravelTicketSearchRequest request,
		CancellationToken cancellationToken);
}
