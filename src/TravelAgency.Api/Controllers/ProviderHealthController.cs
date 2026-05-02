using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Api.Controllers;

[ApiController]
[Route("api/v1/providers/health")]
public class ProviderHealthController(IOptions<AmadeusOptions> amadeus) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { flights = "Amadeus", locations = "Amadeus", hotels = "MockHotelProvider", activities = "MockActivityProvider", amadeusConfigured = !string.IsNullOrWhiteSpace(amadeus.Value.ClientId) });
}
