using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TravelAgency.Application.Providers;

namespace TravelAgency.Api.Controllers;

[ApiController]
[Route("api/v1/locations")]
public class LocationsController(ILocationProvider provider) : ControllerBase
{
    [EnableRateLimiting("locations-relaxed")]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string query, CancellationToken cancellationToken) => Ok(new { items = await provider.SearchLocationsAsync(query, cancellationToken) });
}
