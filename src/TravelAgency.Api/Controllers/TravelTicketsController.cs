using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Api.Controllers;

[ApiController]
[Route("api/v1/travel-tickets")]
public class TravelTicketsController(ITravelTicketService travelTicketService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("search-medium")]
    [HttpPost("search")]
    public async Task<ActionResult<TravelTicketSearchResponse>> Search([FromBody] TravelTicketSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await travelTicketService.SearchAsync(request, cancellationToken);
        return Ok(response);
    }
}
