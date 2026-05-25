using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Providers;

public interface ILocationProvider
{
	Task<IReadOnlyList<LocationSuggestionDto>> SearchLocationsAsync(string query, CancellationToken cancellationToken);
}
