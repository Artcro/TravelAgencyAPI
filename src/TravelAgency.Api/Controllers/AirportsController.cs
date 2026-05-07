using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Services.Airports;

namespace TravelAgency.Api.Controllers;

[ApiController, Route("api/v1/airports")]
public sealed class AirportsController(TravelDbContext db, ILogger<AirportsController> logger) : ControllerBase
{
	[AllowAnonymous, EnableRateLimiting("locations-relaxed"), HttpGet("search")]
	public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int? limit,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
			return BadRequest(new { error = "q must be at least 2 characters" });

		try
		{
			var take = Math.Clamp(limit ?? 8, 1, 20);
			var rawQuery = q.Trim();
			var normalizedQuery = AirportTextNormalizer.Normalize(rawQuery);
			var iataQuery = rawQuery.ToUpperInvariant();

			var items = await db.Airports.AsNoTracking()
				.Where(x => x.IsActive &&
				            (x.IataCode == iataQuery ||
				             x.IataCode.StartsWith(iataQuery) ||
				             (x.CitySearch != null && x.CitySearch.Contains(normalizedQuery)) ||
				             (x.NameSearch != null && x.NameSearch.Contains(normalizedQuery)) ||
				             (x.CountrySearch != null && x.CountrySearch.Contains(normalizedQuery))))
				.Select(x => new
				{
					Airport = x,
					Rank =
						x.IataCode == iataQuery ? 0 :
						x.CitySearch != null && x.CitySearch.StartsWith(normalizedQuery) ? 1 :
						(x.CitySearch != null && x.CitySearch.Contains(normalizedQuery)) ||
						(x.NameSearch != null && x.NameSearch.Contains(normalizedQuery)) ? 2 :
						x.IataCode.StartsWith(iataQuery) ? 3 :
						4
				})
				.OrderBy(x => x.Rank)
				.ThenByDescending(x => x.Airport.ScheduledService)
				.ThenBy(x => x.Airport.City)
				.ThenBy(x => x.Airport.Name)
				.Take(take)
				.Select(x => new AirportAutocompleteDto
				{
					Iata = x.Airport.IataCode,
					Cidade = string.IsNullOrWhiteSpace(x.Airport.City) ? x.Airport.Name : x.Airport.City,
					Pais = PortugueseCountryNames.Resolve(x.Airport.CountryCode, x.Airport.CountryName),
					Nome = x.Airport.Name
				})
				.ToListAsync(cancellationToken);

			return Ok(items);
		}
		catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
		{
			logger.LogError(ex, "Airport autocomplete search failed.");
			return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal error" });
		}
	}
}
