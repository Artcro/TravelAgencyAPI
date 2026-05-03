using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Tests.Travel;

public class FrontendTravelTicketServiceTests
{
	[Fact]
	public async Task Maps_OneWay_Flight_Into_Frontend_Shape_With_Null_Return_Fields()
	{
		var service = CreateService(new StubFlightProvider([
			new FlightOptionDto
			{
				AirlineCode = "LA", AirlineName = "LATAM", TotalPrice = new MoneyDto(3500m, "BRL"),
				OutboundSegments =
				[
					new TripSegmentDto
					{
						Origin = "GRU", Destination = "JFK", DepartureAt = new DateTime(2026, 5, 10, 8, 30, 0),
						ArrivalAt = new DateTime(2026, 5, 10, 12, 0, 0)
					}
				]
			}
		]));

		var item = Assert.Single(await service.SearchAsync(ValidRequest(), default));
		Assert.Equal("LATAM", item.CiaAerea);
		Assert.Equal("08:30", item.HoraPartidaIda);
		Assert.Null(item.HoraPartidaVolta);
		Assert.Equal(0, item.Paradas);
	}

	[Fact]
	public async Task Maps_RoundTrip_Flight_Into_Ida_And_Volta()
	{
		var service = CreateService(new StubFlightProvider([
			new FlightOptionDto
			{
				AirlineCode = "LA", AirlineName = "LATAM", TotalPrice = new MoneyDto(3500m, "BRL"),
				OutboundSegments =
				[
					new TripSegmentDto
					{
						Origin = "GRU", Destination = "JFK", DepartureAt = new DateTime(2026, 5, 10, 8, 30, 0),
						ArrivalAt = new DateTime(2026, 5, 10, 12, 0, 0)
					}
				],
				ReturnSegments =
				[
					new TripSegmentDto
					{
						Origin = "JFK", Destination = "GRU", DepartureAt = new DateTime(2026, 5, 20, 15, 0, 0),
						ArrivalAt = new DateTime(2026, 5, 20, 22, 0, 0)
					}
				]
			}
		]));

		var item = Assert.Single(await service.SearchAsync(ValidRequest(), default));
		Assert.Equal("15:00", item.HoraPartidaVolta);
		Assert.Equal("GRU", item.AeroChegadaVolta);
	}

	[Fact]
	public async Task Id_Is_Sequential_Starting_At_1()
	{
		var service = CreateService(new StubFlightProvider([
			ValidFlight("LA"),
			ValidFlight("AA")
		]));

		var result = await service.SearchAsync(ValidRequest(), default);
		Assert.Equal([1, 2], result.Select(x => x.Id).ToArray());
	}

	[Fact]
	public async Task CiaAerea_Falls_Back_To_AirlineCode()
	{
		var service = CreateService(new StubFlightProvider([ValidFlight("TP", null)]));
		var item = Assert.Single(await service.SearchAsync(ValidRequest(), default));
		Assert.Equal("TP", item.CiaAerea);
	}

	[Fact]
	public async Task Origin_Can_Be_Omitted_When_Default_Configured()
	{
		var service = CreateService(new StubFlightProvider([ValidFlight("TP")]), "GRU");
		var req = ValidRequest();
		req.Origem = "";
		var result = await service.SearchAsync(req, default);
		Assert.Single(result);
	}

	[Fact]
	public async Task Origin_Missing_Returns_Validation_Error_Without_Default()
	{
		var service = CreateService(new StubFlightProvider([ValidFlight("TP")]));
		var req = ValidRequest();
		req.Origem = "";
		await Assert.ThrowsAsync<ArgumentException>(() => service.SearchAsync(req, default));
	}

	[Fact]
	public async Task Response_Serializes_Using_Expected_CamelCase_Names()
	{
		var service = CreateService(new StubFlightProvider([ValidFlight("LA")]));
		var response = await service.SearchAsync(ValidRequest(), default);
		var json = JsonSerializer.Serialize(response,
			new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

		Assert.Contains("\"ciaAerea\"", json);
		Assert.Contains("\"horaPartidaIda\"", json);
		Assert.Contains("\"dataChegadaVolta\"", json);
		Assert.DoesNotContain("\"items\"", json);
	}

	private static FrontendTravelTicketService CreateService(IFlightProvider provider, string? defaultOrigin = null)
	{
		return new FrontendTravelTicketService(provider, new TravelTicketSearchRequestValidator(),
			Options.Create(new TravelSearchDefaultsOptions { DefaultOrigin = defaultOrigin }),
			NullLogger<FrontendTravelTicketService>.Instance);
	}

	private static FrontendTravelTicketSearchRequest ValidRequest()
	{
		return new FrontendTravelTicketSearchRequest
			{ Origem = "GRU", Destino = "JFK", DataIda = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
	}

	private static FlightOptionDto ValidFlight(string airlineCode, string? airlineName = "LATAM")
	{
		return new FlightOptionDto
		{
			AirlineCode = airlineCode,
			AirlineName = airlineName,
			TotalPrice = new MoneyDto(3500m, "BRL"),
			OutboundSegments =
			[
				new TripSegmentDto
				{
					Origin = "GRU", Destination = "JFK", DepartureAt = new DateTime(2026, 5, 10, 8, 30, 0),
					ArrivalAt = new DateTime(2026, 5, 10, 12, 0, 0)
				}
			]
		};
	}

	private sealed class StubFlightProvider(IReadOnlyList<FlightOptionDto> flights) : IFlightProvider
	{
		public Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request,
			CancellationToken cancellationToken)
		{
			return Task.FromResult(flights);
		}
	}
}