using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Api.Controllers;

[ApiController]
[Route("api/v1/saved-trips")]
public class SavedTripsController(ISavedTripService service) : ControllerBase
{
    [HttpPost] public async Task<IActionResult> Save([FromBody] SaveTripRequest req, CancellationToken ct) => Ok(await service.SaveAsync(req, null, ct));
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await service.ListAsync(null, ct));
    [HttpGet("{tripId:guid}")] public async Task<IActionResult> Get(Guid tripId, CancellationToken ct) => Ok(await service.GetByIdAsync(tripId, null, ct));
    [HttpDelete("{tripId:guid}")] public async Task<IActionResult> Delete(Guid tripId, CancellationToken ct) => await service.DeleteAsync(tripId, null, ct) ? NoContent() : NotFound();
}
