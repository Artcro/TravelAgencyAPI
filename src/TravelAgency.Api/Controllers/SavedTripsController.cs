using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure;

namespace TravelAgency.Api.Controllers;

[ApiController]
[Route("api/v1/saved-trips")]
public class SavedTripsController(ISavedTripService service, IOptions<SecurityOptions> security) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveTripRequest req, CancellationToken ct)
    {
        if (security.Value.RequireAuthentication && User.Identity?.IsAuthenticated != true) return Unauthorized();
        return Ok(await service.SaveAsync(req, null, ct));
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (security.Value.RequireAuthentication && User.Identity?.IsAuthenticated != true) return Unauthorized();
        return Ok(await service.ListAsync(null, ct));
    }

    [HttpGet("{tripId:guid}")]
    public async Task<IActionResult> Get(Guid tripId, CancellationToken ct)
    {
        if (security.Value.RequireAuthentication && User.Identity?.IsAuthenticated != true) return Unauthorized();
        return Ok(await service.GetByIdAsync(tripId, null, ct));
    }

    [HttpDelete("{tripId:guid}")]
    public async Task<IActionResult> Delete(Guid tripId, CancellationToken ct)
    {
        if (security.Value.RequireAuthentication && User.Identity?.IsAuthenticated != true) return Unauthorized();
        return await service.DeleteAsync(tripId, null, ct) ? NoContent() : NotFound();
    }
}
