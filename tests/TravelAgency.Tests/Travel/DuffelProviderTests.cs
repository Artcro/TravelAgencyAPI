using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Providers.Duffel;
using TravelAgency.Infrastructure.Providers.Mock;

namespace TravelAgency.Tests.Travel;

public class DuffelProviderTests
{
    [Fact]
    public async Task Maps_Duffel_RoundTrip_Offers()
    {
        var provider = CreateProvider(SampleResponse());
        var req = new TripSearchRequest { Origin = "JFK", Destination = "LHR", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), Adults = 1, Currency = "USD", MaxFlightResults = 5 };
        var results = await provider.SearchFlightsAsync(req, default);
        Assert.Single(results);
    }

    [Fact]
    public async Task Duffel_422_Includes_Truncated_Body()
    {
        var body = "{\"errors\":[{\"title\":\"validation\"}]}";
        var provider = CreateProvider(body, HttpStatusCode.UnprocessableEntity);
        var req = new TripSearchRequest { Origin = "JFK", Destination = "LHR", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), Adults = 1, Currency = "USD", MaxFlightResults = 5 };
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.SearchFlightsAsync(req, default));
        Assert.Contains("422", ex.Message);
        Assert.Contains("validation", ex.Message);
        Assert.DoesNotContain("token", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Duffel_Large_ContentLength_Fails_Cleanly()
    {
        var provider = CreateProvider(SampleResponse(), HttpStatusCode.Created, contentLength: 9_000_000);
        var req = new TripSearchRequest { Origin = "JFK", Destination = "LHR", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), Adults = 1, Currency = "USD", MaxFlightResults = 5 };
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.SearchFlightsAsync(req, default));
        Assert.Contains("response too large", ex.Message);
    }

    [Fact]
    public async Task MockLocationProvider_Returns_Useful_Matches()
    {
        var provider = new MockLocationProvider();
        var results = await provider.SearchLocationsAsync("rio", default);
        Assert.Contains(results, x => x.Code == "RIO" || x.Code == "GIG");
    }

    static DuffelFlightProvider CreateProvider(string body, HttpStatusCode code = HttpStatusCode.OK, long? contentLength = null)
    {
        var db = new TravelDbContext(new DbContextOptionsBuilder<TravelDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var http = new HttpClient(new StubHandler(body, code, contentLength));
        var factory = new StubFactory(http);
        return new DuffelFlightProvider(factory, Options.Create(new DuffelOptions { BaseUrl = "https://api.duffel.com", AccessToken = "token", Version = "v2", MaxResponseBytes = 5_242_880 }), db, NullLogger<DuffelFlightProvider>.Instance);
    }

    static string SampleResponse() => """
    {"data":{"offers":[{"id":"off_1","total_amount":"123.45","total_currency":"USD","owner":{"iata_code":"AA","name":"American Airlines"},"slices":[{"duration":"PT7H","segments":[{"origin":{"iata_code":"JFK"},"destination":{"iata_code":"LHR"},"departing_at":"2026-10-10T10:00:00Z","arriving_at":"2026-10-10T17:00:00Z","duration":"PT7H","marketing_carrier":{"iata_code":"AA"},"marketing_carrier_flight_number":"100"}]},{"duration":"PT8H","segments":[{"origin":{"iata_code":"LHR"},"destination":{"iata_code":"JFK"},"departing_at":"2026-10-15T10:00:00Z","arriving_at":"2026-10-15T18:00:00Z","duration":"PT8H","marketing_carrier":{"iata_code":"AA"},"marketing_carrier_flight_number":"101"}]}]}]}}
    """;

    sealed class StubFactory(HttpClient client) : IHttpClientFactory { public HttpClient CreateClient(string name) => client; }
    sealed class StubHandler(string body, HttpStatusCode code, long? contentLength) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(code) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
            if (contentLength.HasValue) response.Content.Headers.ContentLength = contentLength.Value;
            return Task.FromResult(response);
        }
    }
}
