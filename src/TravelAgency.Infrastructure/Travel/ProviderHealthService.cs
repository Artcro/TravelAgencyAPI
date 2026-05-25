using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Infrastructure.Travel;

public sealed class ProviderHealthService(IOptions<TravelProvidersOptions> providers) : IProviderHealthService
{
	public ProviderHealthDto GetCurrent()
	{
		return new ProviderHealthDto(
			Flights: "Duffel",
			Locations: providers.Value.IsLocalLocationProvider() ? "LocalAirportDatabase" : "Mock",
			Hotels: "MockHotelProvider",
			Activities: "MockActivityProvider");
	}
}
