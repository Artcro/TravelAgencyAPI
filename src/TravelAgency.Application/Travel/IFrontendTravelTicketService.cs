using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface IFrontendTravelTicketService
{
	Task<IReadOnlyList<FrontendTravelTicketDto>> SearchAsync(FrontendTravelTicketSearchRequest request,
		CancellationToken cancellationToken);
}
