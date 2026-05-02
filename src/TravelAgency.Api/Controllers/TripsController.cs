using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Api.Controllers;

[ApiController]
[Route("api/v1/trips")]
public class TripsController(ITripSearchService tripSearchService) : ControllerBase
{
    [EnableRateLimiting("search-medium")]
    [HttpPost("search")]
    public async Task<ActionResult<TripSearchResponse>> Search([FromBody] TripSearchRequest req, CancellationToken cancellationToken)
    {
        var result = await tripSearchService.SearchAsync(req, null, cancellationToken);
        return Ok(result);
    }

    [HttpGet("searches/{searchId:guid}")]
    public async Task<ActionResult<TripSearchResponse>> GetSearch(Guid searchId, CancellationToken cancellationToken)
    {
        var result = await tripSearchService.GetSearchByIdAsync(searchId, null, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
