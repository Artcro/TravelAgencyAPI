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
    public void DuffelOptions_Defaults_Are_Correct()
    {
        var options = new DuffelOptions();
        Assert.Equal(45, options.TimeoutSeconds);
        Assert.Equal(15_000, options.SupplierTimeoutMilliseconds);
        Assert.Equal(1, options.MaxConnections);
        Assert.Equal(10, options.MaxOffersToRead);
        Assert.True(options.UseReturnOffers);
        Assert.True(options.ReturnEmptyOnTimeout);
    }

    [Fact]
    public async Task Sends_SupplierTimeout_And_MaxConnections_And_Uses_ListOffers_Flow()
    {
        var handler = new SequenceHandler(
            _ => new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("""{"data":{"id":"orq_123"}}""") },
            req =>
            {
                Assert.Contains("offer_request_id=orq_123", req.RequestUri!.Query);
                Assert.Contains("limit=2", req.RequestUri!.Query);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ListOffersResponse(3), Encoding.UTF8, "application/json") };
            });

        var provider = CreateProvider(handler, new DuffelOptions { BaseUrl = "https://api.duffel.com", AccessToken = "token", Version = "v2", MaxOffersToRead = 2, UseReturnOffers = false });
        var req = BasicRequest();
        var results = await provider.SearchFlightsAsync(req, default);

        var postBody = handler.RequestBodies[0];
        Assert.Contains("\"supplier_timeout\":15000", postBody);
        Assert.Contains("\"max_connections\":1", postBody);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task Timeout_With_ReturnEmptyOnTimeout_True_Returns_Empty()
    {
        var handler = new ThrowingHandler(new TaskCanceledException("timeout"));
        var provider = CreateProvider(handler, new DuffelOptions { BaseUrl = "https://api.duffel.com", AccessToken = "token", ReturnEmptyOnTimeout = true });
        var results = await provider.SearchFlightsAsync(BasicRequest(), default);
        Assert.Empty(results);
    }

    [Fact]
    public async Task Timeout_With_ReturnEmptyOnTimeout_False_Throws_Clean_Message()
    {
        var handler = new ThrowingHandler(new TaskCanceledException("timeout"));
        var provider = CreateProvider(handler, new DuffelOptions { BaseUrl = "https://api.duffel.com", AccessToken = "token", ReturnEmptyOnTimeout = false, TimeoutSeconds = 45 });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.SearchFlightsAsync(BasicRequest(), default));
        Assert.Equal("Duffel flight search timed out after 45 seconds.", ex.Message);
    }

    [Fact]
    public async Task Duffel_422_Includes_Truncated_Sanitized_Body()
    {
        var body = "{\"errors\":[{\"title\":\"validation\",\"token\":\"abc\"}]}";
        var provider = CreateProvider(new StaticHandler(body, HttpStatusCode.UnprocessableEntity), new DuffelOptions { BaseUrl = "https://api.duffel.com", AccessToken = "token" });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.SearchFlightsAsync(BasicRequest(), default));
        Assert.Contains("422", ex.Message);
        Assert.Contains("validation", ex.Message);
        Assert.DoesNotContain("token", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MockLocationProvider_Returns_Useful_Matches()
    {
        var provider = new MockLocationProvider();
        var results = await provider.SearchLocationsAsync("rio", default);
        Assert.Contains(results, x => x.Code == "RIO" || x.Code == "GIG");
    }

    static TripSearchRequest BasicRequest() => new() { Origin = "JFK", Destination = "LHR", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), Adults = 1, Currency = "USD", MaxFlightResults = 5 };

    static DuffelFlightProvider CreateProvider(HttpMessageHandler handler, DuffelOptions opts)
    {
        var db = new TravelDbContext(new DbContextOptionsBuilder<TravelDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var http = new HttpClient(handler);
        var factory = new StubFactory(http);
        return new DuffelFlightProvider(factory, Options.Create(opts), db, NullLogger<DuffelFlightProvider>.Instance);
    }

    static string ListOffersResponse(int count)
    {
        var offers = string.Join(',', Enumerable.Range(1, count).Select(i => $"{{\"id\":\"off_{i}\",\"total_amount\":\"100.00\",\"total_currency\":\"USD\",\"owner\":{{\"iata_code\":\"AA\",\"name\":\"American\"}},\"slices\":[{{\"duration\":\"PT7H\",\"segments\":[{{\"origin\":{{\"iata_code\":\"JFK\"}},\"destination\":{{\"iata_code\":\"LHR\"}},\"departing_at\":\"2026-10-10T10:00:00Z\",\"arriving_at\":\"2026-10-10T17:00:00Z\",\"duration\":\"PT7H\",\"marketing_carrier\":{{\"iata_code\":\"AA\"}},\"marketing_carrier_flight_number\":\"100\"}}]}}]}}"));
        return $"{{\"data\":[{offers}]}}";
    }

    sealed class StubFactory(HttpClient client) : IHttpClientFactory { public HttpClient CreateClient(string name) => client; }
    sealed class StaticHandler(string body, HttpStatusCode code) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    }
    sealed class ThrowingHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromException<HttpResponseMessage>(exception);
    }

    sealed class SequenceHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] responses) : HttpMessageHandler
    {
        private int _index;
        public List<string> RequestBodies { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content is not null) RequestBodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
            return responses[_index++](request);
        }
    }
}
