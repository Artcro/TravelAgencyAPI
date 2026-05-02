using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Providers.Mock;

namespace TravelAgency.Tests.Travel;

public class TravelPhase2Tests
{
    [Fact]
    public void TripValidation_Fails_InvalidRequest()
    {
        var validator = new TripSearchRequestValidator();
        var errors = validator.Validate(new TripSearchRequest { Origin = "RIO", Destination = "RIO", Adults = 0, MaxFlightResults = 99 });
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task MockHotelProvider_Returns_Results()
    {
        var provider = new MockHotelProvider();
        var results = await provider.SearchHotelsAsync(new TripSearchRequest { Destination = "RIO", Currency = "BRL" }, default);
        Assert.True(results.Count >= 3);
    }

    [Fact]
    public async Task MockActivityProvider_Returns_Results()
    {
        var provider = new MockActivityProvider();
        var results = await provider.SearchActivitiesAsync(new TripSearchRequest { Destination = "RIO", Currency = "BRL" }, default);
        Assert.True(results.Count >= 3);
    }
}
