using Microsoft.AspNetCore.Mvc;
using TravelAgency.Api.Config;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Api.Controllers;

[ApiController, Route("api/v1/travel-packages")]
public sealed class TravelPackagesController(
	ITravelPackageService service,
	ICurrentUserService currentUserService) : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> Save([FromBody] SaveTravelPackageRequest request, CancellationToken ct)
	{
		var userId = currentUserService.UserId;
		if (userId is null) return Unauthorized();

		var result = await service.SaveAsync(userId.Value, request, ct);
		if (!result.IsValid) return BadRequest(ValidationProblemBuilder.Build(result.Errors, HttpContext.Request.Path));
		return Ok(result.Value);
	}

	[HttpGet]
	public async Task<IActionResult> List(CancellationToken ct)
	{
		var userId = currentUserService.UserId;
		if (userId is null) return Unauthorized();

		return Ok(await service.ListAsync(userId.Value, ct));
	}

	[HttpPost("{id:guid}/cancel")]
	public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
	{
		var userId = currentUserService.UserId;
		if (userId is null) return Unauthorized();

		var dto = await service.CancelAsync(userId.Value, id, ct);
		return dto is null ? NotFound() : Ok(dto);
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		var userId = currentUserService.UserId;
		if (userId is null) return Unauthorized();

		return await service.DeleteAsync(userId.Value, id, ct) ? NoContent() : NotFound();
	}
}
