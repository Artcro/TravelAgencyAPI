using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Api.Controllers;

[ApiController, Route("api/v1/providers/health")]
public class ProviderHealthController(IOptions<TravelProvidersOptions> providers) : ControllerBase
{
	[HttpGet]
	public IActionResult Get()
	{
		var locationProvider = IsLocalLocationProvider(providers.Value.LocationProvider) ? "LocalAirportDatabase" : "Mock";
		return Ok(new
		{
			flights = "Duffel",
			locations = locationProvider,
			hotels = "MockHotelProvider",
			activities = "MockActivityProvider"
		});
	}

	private static bool IsLocalLocationProvider(string provider)
	{
		return string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase) ||
		       string.Equals(provider, "Database", StringComparison.OrdinalIgnoreCase) ||
		       string.Equals(provider, "OurAirports", StringComparison.OrdinalIgnoreCase);
	}
}
