using Microsoft.EntityFrameworkCore;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Infrastructure.Database;

namespace TravelAgency.Infrastructure.Providers.Local;

public sealed class LocalAirportLocationProvider(TravelDbContext db) : ILocationProvider
{
	public async Task<IReadOnlyList<LocationSuggestionDto>> SearchLocationsAsync(string query,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(query)) return [];

		var q = query.Trim().ToLowerInvariant();
		if (q.Length < 2) return [];

		var matches = await db.Airports.AsNoTracking()
			.Where(x => x.IsActive &&
			            (x.IataCode.ToLower().Contains(q) ||
			             x.Name.ToLower().Contains(q) ||
			             (x.City != null && x.City.ToLower().Contains(q)) ||
			             x.CountryName.ToLower().Contains(q) ||
			             x.CountryCode.ToLower().Contains(q)))
			.Select(x => new
			{
				Airport = x,
				Rank =
					x.IataCode.ToLower() == q ? 0 :
					x.IataCode.ToLower().StartsWith(q) ? 1 :
					x.City != null && x.City.ToLower().StartsWith(q) ? 2 :
					x.Name.ToLower().StartsWith(q) ? 3 :
					4
			})
			.OrderBy(x => x.Rank)
			.ThenByDescending(x => x.Airport.ScheduledService)
			.ThenBy(x => x.Airport.City)
			.ThenBy(x => x.Airport.Name)
			.Take(12)
			.ToListAsync(cancellationToken);

		return matches.Select(x =>
		{
			var airport = x.Airport;
			var displayPrefix = string.IsNullOrWhiteSpace(airport.City)
				? airport.Name
				: $"{airport.City} - {airport.Name}";

			return new LocationSuggestionDto(
				airport.IataCode,
				airport.Name,
				"AIRPORT",
				airport.CountryCode,
				$"{displayPrefix} ({airport.IataCode})",
				airport.City,
				airport.CountryName);
		}).ToList();
	}
}
