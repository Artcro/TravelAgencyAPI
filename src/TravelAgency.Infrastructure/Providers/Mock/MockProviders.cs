using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;

namespace TravelAgency.Infrastructure.Providers.Mock;

public sealed class MockHotelProvider : IHotelProvider
{
    public Task<IReadOnlyList<HotelOptionDto>> SearchHotelsAsync(TripSearchRequest request, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<HotelOptionDto>>([
        new() { Provider="Mock", ProviderHotelId="H1", Name=$"{request.Destination} Grand Hotel", CityCode=request.Destination, Rating=5, PricePerNight=new MoneyDto(520, request.Currency) },
        new() { Provider="Mock", ProviderHotelId="H2", Name=$"{request.Destination} Boutique", CityCode=request.Destination, Rating=4, PricePerNight=new MoneyDto(350, request.Currency) },
        new() { Provider="Mock", ProviderHotelId="H3", Name=$"{request.Destination} Budget Inn", CityCode=request.Destination, Rating=3, PricePerNight=new MoneyDto(190, request.Currency) }
    ]);
}
public sealed class MockActivityProvider : IActivityProvider
{
    public Task<IReadOnlyList<ActivityOptionDto>> SearchActivitiesAsync(TripSearchRequest request, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ActivityOptionDto>>([
        new() { Provider="Mock", ProviderActivityId="A1", Title=$"City tour in {request.Destination}", Description="Guided highlights", Price=new MoneyDto(120, request.Currency) },
        new() { Provider="Mock", ProviderActivityId="A2", Title=$"Food experience in {request.Destination}", Description="Local cuisine tasting", Price=new MoneyDto(180, request.Currency) },
        new() { Provider="Mock", ProviderActivityId="A3", Title=$"Museum pass {request.Destination}", Description="Top museums", Price=new MoneyDto(75, request.Currency) }
    ]);
}
