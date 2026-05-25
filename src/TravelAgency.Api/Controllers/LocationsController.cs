using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TravelAgency.Application.Providers;

namespace TravelAgency.Api.Controllers;

/// <summary>
/// Legacy location-search endpoint, duplicated by
/// <see cref="AirportsController"/>. Scheduled for removal once the frontend
/// confirms no callers remain on <c>/api/v1/locations</c>.
/// </summary>
[Obsolete("Use /api/v1/airports/search instead. Slated for removal after 2026-08-01.")]
[ApiController, Route("api/v1/locations")]
public class LocationsController(ILocationProvider provider) : ControllerBase
{
	[EnableRateLimiting("locations-relaxed"), HttpGet]
	public async Task<IActionResult> Get([FromQuery] string query, CancellationToken cancellationToken)
	{
		return Ok(new { items = await provider.SearchLocationsAsync(query, cancellationToken) });
	}
}
