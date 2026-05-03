using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;

namespace TravelAgency.Infrastructure.Providers.Mock;

public sealed class MockHotelProvider : IHotelProvider
{
	public Task<IReadOnlyList<HotelOptionDto>> SearchHotelsAsync(TripSearchRequest request,
		CancellationToken cancellationToken)
	{
		return Task.FromResult<IReadOnlyList<HotelOptionDto>>([
			new HotelOptionDto
			{
				Provider = "Mock", ProviderHotelId = "H1", Name = $"{request.Destination} Grand Hotel",
				CityCode = request.Destination, Rating = 5, PricePerNight = new MoneyDto(520, request.Currency)
			},
			new HotelOptionDto
			{
				Provider = "Mock", ProviderHotelId = "H2", Name = $"{request.Destination} Boutique",
				CityCode = request.Destination, Rating = 4, PricePerNight = new MoneyDto(350, request.Currency)
			},
			new HotelOptionDto
			{
				Provider = "Mock", ProviderHotelId = "H3", Name = $"{request.Destination} Budget Inn",
				CityCode = request.Destination, Rating = 3, PricePerNight = new MoneyDto(190, request.Currency)
			}
		]);
	}
}

public sealed class MockActivityProvider : IActivityProvider
{
	public Task<IReadOnlyList<ActivityOptionDto>> SearchActivitiesAsync(TripSearchRequest request,
		CancellationToken cancellationToken)
	{
		return Task.FromResult<IReadOnlyList<ActivityOptionDto>>([
			new ActivityOptionDto
			{
				Provider = "Mock", ProviderActivityId = "A1", Title = $"City tour in {request.Destination}",
				Description = "Guided highlights", Price = new MoneyDto(120, request.Currency)
			},
			new ActivityOptionDto
			{
				Provider = "Mock", ProviderActivityId = "A2", Title = $"Food experience in {request.Destination}",
				Description = "Local cuisine tasting", Price = new MoneyDto(180, request.Currency)
			},
			new ActivityOptionDto
			{
				Provider = "Mock", ProviderActivityId = "A3", Title = $"Museum pass {request.Destination}",
				Description = "Top museums", Price = new MoneyDto(75, request.Currency)
			}
		]);
	}
}

public sealed class MockLocationProvider : ILocationProvider
{
	private static readonly List<LocationSuggestionDto> Seed =
	[
		new("RIO", "Rio de Janeiro", "CITY", "BR", "Rio de Janeiro (RIO)"),
		new("GIG", "Rio de Janeiro/Galeao", "AIRPORT", "BR", "Rio de Janeiro/Galeao (GIG)"),
		new("SAO", "Sao Paulo", "CITY", "BR", "Sao Paulo (SAO)"),
		new("LIS", "Lisbon", "CITY", "PT", "Lisbon (LIS)"),
		new("LON", "London", "CITY", "GB", "London (LON)"),
		new("PAR", "Paris", "CITY", "FR", "Paris (PAR)"),
		new("NYC", "New York", "CITY", "US", "New York (NYC)"),
		new("MIA", "Miami", "CITY", "US", "Miami (MIA)"),
		new("MAD", "Madrid", "CITY", "ES", "Madrid (MAD)"),
		new("BUE", "Buenos Aires", "CITY", "AR", "Buenos Aires (BUE)")
	];

	public Task<IReadOnlyList<LocationSuggestionDto>> SearchLocationsAsync(string query,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(query)) return Task.FromResult<IReadOnlyList<LocationSuggestionDto>>([]);
		var q = query.Trim().ToLowerInvariant();
		var items = Seed.Where(x =>
			x.DisplayName.ToLowerInvariant().Contains(q) || x.Code.ToLowerInvariant().Contains(q) ||
			x.Name.ToLowerInvariant().Contains(q)).Take(8).ToList();

		return Task.FromResult<IReadOnlyList<LocationSuggestionDto>>(items);
	}
}