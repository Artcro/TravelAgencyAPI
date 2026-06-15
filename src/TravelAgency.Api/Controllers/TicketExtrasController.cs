using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TravelAgency.Api.Config;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Api.Controllers;

[ApiController, Route("api/v1/travel-tickets")]
public sealed class TicketExtrasController(ITicketExtrasService ticketExtrasService) : ControllerBase
{
	[AllowAnonymous, EnableRateLimiting("search-medium"), HttpGet("extras")]
	public async Task<ActionResult<TicketExtrasDto>> Extras(
		[FromQuery] TicketExtrasQueryRequest query, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(query.Destino)) return BadRequest(new { error = "destino is required" });

		var result = await ticketExtrasService.GetExtrasAsync(query.Destino, query.Moeda, cancellationToken);
		if (!result.IsValid) return BadRequest(ValidationProblemBuilder.Build(result.Errors, HttpContext.Request.Path));
		return Ok(result.Value);
	}
}
