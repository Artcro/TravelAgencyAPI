using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface IProviderHealthService
{
	ProviderHealthDto GetCurrent();
}
