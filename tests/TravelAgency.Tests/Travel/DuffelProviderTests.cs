using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Options;
using TravelAgency.Infrastructure.Providers.Duffel;
using TravelAgency.Infrastructure.Providers.Mock;

namespace TravelAgency.Tests.Travel;

public class DuffelProviderTests
{
    [Fact]
    public void TravelProviders_Defaults_To_Duffel_And_Mock()
    {
        var options = new TravelProvidersOptions();
        Assert.Equal("Duffel", options.FlightProvider);
        Assert.Equal("Mock", options.LocationProvider);
    }

    [Fact]
    public async Task Maps_Duffel_RoundTrip_Offers()
    {
        var provider = CreateProvider(SampleResponse());
        var req = new TripSearchRequest { Origin = "JFK", Destination = "LHR", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), Adults = 1, Currency = "USD", MaxFlightResults = 5 };
        var results = await provider.SearchFlightsAsync(req, default);
        var flight = Assert.Single(results);
        Assert.Equal("Duffel", flight.Provider);
        Assert.Single(flight.OutboundSegments);
        Assert.Single(flight.ReturnSegments);
        Assert.Equal("AA", flight.OutboundSegments[0].CarrierCode);
    }

    [Fact]
    public async Task Malformed_Duffel_Response_Does_Not_Throw()
    {
        var provider = CreateProvider("{\"data\":{}}", HttpStatusCode.OK);
        var req = new TripSearchRequest { Origin = "JFK", Destination = "LHR", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), Adults = 1, Currency = "USD", MaxFlightResults = 5 };
        var results = await provider.SearchFlightsAsync(req, default);
        Assert.Empty(results);
    }

    [Fact]
    public async Task MockLocationProvider_Returns_Useful_Matches()
    {
        var provider = new MockLocationProvider();
        var results = await provider.SearchLocationsAsync("rio", default);
        Assert.Contains(results, x => x.Code == "RIO" || x.Code == "GIG");
    }

    static DuffelFlightProvider CreateProvider(string body, HttpStatusCode code = HttpStatusCode.OK)
    {
        var db = new TravelDbContext(new DbContextOptionsBuilder<TravelDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var http = new HttpClient(new StubHandler(body, code));
        var factory = new StubFactory(http);
        return new DuffelFlightProvider(factory, Options.Create(new DuffelOptions { BaseUrl = "https://api.duffel.com", AccessToken = "token", Version = "v2" }), db);
    }

    static string SampleResponse() => """
    {"data":{"offers":[{"id":"off_1","total_amount":"123.45","total_currency":"USD","owner":{"iata_code":"AA","name":"American Airlines"},"slices":[{"duration":"PT7H","segments":[{"origin":{"iata_code":"JFK"},"destination":{"iata_code":"LHR"},"departing_at":"2026-10-10T10:00:00Z","arriving_at":"2026-10-10T17:00:00Z","duration":"PT7H","marketing_carrier":{"iata_code":"AA"},"marketing_carrier_flight_number":"100"}]},{"duration":"PT8H","segments":[{"origin":{"iata_code":"LHR"},"destination":{"iata_code":"JFK"},"departing_at":"2026-10-15T10:00:00Z","arriving_at":"2026-10-15T18:00:00Z","duration":"PT8H","marketing_carrier":{"iata_code":"AA"},"marketing_carrier_flight_number":"101"}]}]}]}}
    """;

    sealed class StubFactory(HttpClient client) : IHttpClientFactory { public HttpClient CreateClient(string name) => client; }
    sealed class StubHandler(string body, HttpStatusCode code) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    }
}
