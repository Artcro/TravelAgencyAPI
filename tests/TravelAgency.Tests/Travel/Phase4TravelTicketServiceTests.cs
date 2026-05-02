using Microsoft.Extensions.Logging.Abstractions;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure;

namespace TravelAgency.Tests.Travel;

public class Phase4TravelTicketServiceTests
{
    [Fact]
    public async Task Maps_One_Segment_Correctly()
    {
        var service = CreateService(new StubFlightProvider([
            new FlightOptionDto
            {
                Provider = "Amadeus", ProviderOfferId = "1", AirlineCode = "TP", AirlineName = "TAP Air Portugal", TotalPrice = new MoneyDto(4720.50m, "BRL"),
                OutboundSegments = [new TripSegmentDto { Origin = "GIG", Destination = "LIS", DepartureAt = new DateTime(2026, 8, 10, 21, 30, 0), ArrivalAt = new DateTime(2026, 8, 11, 11, 10, 0) }]
            }
        ]));

        var result = await service.SearchAsync(ValidRequest(), default);
        var item = Assert.Single(result.Items);
        Assert.Equal("GIG", item.DepartureAirportCode);
        Assert.Equal("21:30", item.DepartureTime);
        Assert.Equal("LIS", item.ArrivalAirportCode);
        Assert.Equal("11:10", item.ArrivalTime);
        Assert.Equal("2026-08-11", item.ArrivalDate);
        Assert.Equal(0, item.Stops);
    }

    [Fact]
    public async Task Maps_Multiple_Segments_And_Calculates_Stops()
    {
        var service = CreateService(new StubFlightProvider([
            new FlightOptionDto
            {
                ProviderOfferId = "2", AirlineCode = "TP", TotalPrice = new MoneyDto(3000, "BRL"),
                OutboundSegments =
                [
                    new TripSegmentDto { Origin = "SDU", Destination = "GRU", DepartureAt = new DateTime(2026, 8, 10, 10, 0, 0), ArrivalAt = new DateTime(2026, 8, 10, 11, 0, 0) },
                    new TripSegmentDto { Origin = "GRU", Destination = "LIS", DepartureAt = new DateTime(2026, 8, 10, 13, 0, 0), ArrivalAt = new DateTime(2026, 8, 11, 4, 30, 0) }
                ]
            }
        ]));

        var item = Assert.Single((await service.SearchAsync(ValidRequest(), default)).Items);
        Assert.Equal(1, item.Stops);
        Assert.Equal("SDU", item.DepartureAirportCode);
        Assert.Equal("LIS", item.ArrivalAirportCode);
    }

    [Fact]
    public async Task Uses_AirlineCode_When_AirlineName_Missing()
    {
        var service = CreateService(new StubFlightProvider([
            new FlightOptionDto { AirlineCode = "TP", AirlineName = null, ProviderOfferId = "x", TotalPrice = new MoneyDto(1, "BRL"), OutboundSegments = [new TripSegmentDto { Origin = "GIG", Destination = "LIS", DepartureAt = DateTime.UtcNow.AddDays(5), ArrivalAt = DateTime.UtcNow.AddDays(5).AddHours(8) }] }
        ]));

        var item = Assert.Single((await service.SearchAsync(ValidRequest(), default)).Items);
        Assert.Equal("TP", item.AirlineName);
    }

    [Fact]
    public async Task Rejects_Same_Origin_Destination()
    {
        var service = CreateService(new StubFlightProvider([]));
        var req = ValidRequest();
        req.Destination = req.Origin;
        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync(req, default));
    }

    [Fact]
    public async Task Rejects_Past_Departure_Date()
    {
        var service = CreateService(new StubFlightProvider([]));
        var req = ValidRequest();
        req.DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync(req, default));
    }

    [Fact]
    public async Task Returns_Empty_Items_When_No_Flights()
    {
        var service = CreateService(new StubFlightProvider([]));
        var result = await service.SearchAsync(ValidRequest(), default);
        Assert.Empty(result.Items);
    }

    private static TravelTicketService CreateService(IFlightProvider flightProvider) => new(new TravelTicketSearchRequestValidator(), flightProvider, NullLogger<TravelTicketService>.Instance);
    private static TravelTicketSearchRequest ValidRequest() => new() { Origin = "RIO", Destination = "LIS", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), Adults = 1, MaxResults = 10 };

    private sealed class StubFlightProvider(IReadOnlyList<FlightOptionDto> flights) : IFlightProvider
    {
        public Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request, CancellationToken cancellationToken) => Task.FromResult(flights);
    }
}
