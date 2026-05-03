using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Api.Controllers;

[ApiController]
[Route("api/v1/frontend/travel-tickets")]
public sealed class FrontendTravelTicketsController(IFrontendTravelTicketService frontendTravelTicketService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("search-medium")]
    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<FrontendTravelTicketDto>>> Search([FromBody] FrontendTravelTicketSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await frontendTravelTicketService.SearchAsync(request, cancellationToken);
        return Ok(response);
    }
}
