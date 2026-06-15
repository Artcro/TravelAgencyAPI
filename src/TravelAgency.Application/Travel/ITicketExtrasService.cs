using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface ITicketExtrasService
{
	Task<Result<TicketExtrasDto>> GetExtrasAsync(string destino, string? moeda, CancellationToken cancellationToken);
}
