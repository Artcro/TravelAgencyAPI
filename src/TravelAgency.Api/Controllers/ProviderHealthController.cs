using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Api.Controllers;

[ApiController, Route("api/v1/providers/health")]
public class ProviderHealthController(IOptions<AmadeusOptions> amadeus, IOptions<TravelProvidersOptions> providers)
	: ControllerBase
{
	[HttpGet]
	public IActionResult Get()
	{
		var locationProvider = providers.Value.LocationProvider;
		if (IsLocalLocationProvider(locationProvider))
		{
			locationProvider = "LocalAirportDatabase";
		}
		else if (string.Equals(locationProvider, "Amadeus", StringComparison.OrdinalIgnoreCase) &&
		         !string.IsNullOrWhiteSpace(amadeus.Value.ClientId) &&
		         !string.IsNullOrWhiteSpace(amadeus.Value.ClientSecret))
		{
			locationProvider = "Amadeus";
		}
		else
		{
			locationProvider = "Mock";
		}

		return Ok(new
		{
			flights = string.Equals(providers.Value.FlightProvider, "Amadeus", StringComparison.OrdinalIgnoreCase)
				? "Amadeus"
				: "Duffel",
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
