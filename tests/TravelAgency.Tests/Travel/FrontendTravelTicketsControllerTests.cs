using Microsoft.AspNetCore.Mvc;
using TravelAgency.Api.Controllers;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Tests.Travel;

public sealed class FrontendTravelTicketsControllerTests
{
	[Fact]
	public async Task SearchGet_Maps_Minimal_Query_With_Defaults()
	{
		var service = new CapturingFrontendService();
		var sut = new FrontendTravelTicketsController(service);
		var query = new FrontendTravelTicketQueryRequest
		{
			Destino = "JFK",
			DataIda = new DateOnly(2026, 5, 10)
		};

		var result = await sut.SearchGet(query, CancellationToken.None);

		var ok = Assert.IsType<OkObjectResult>(result.Result);
		Assert.Same(service.Response, ok.Value);
		Assert.NotNull(service.LastRequest);
		Assert.Equal("JFK", service.LastRequest!.Destino);
		Assert.Equal(new DateOnly(2026, 5, 10), service.LastRequest.DataIda);
		Assert.Null(service.LastRequest.Origem);
		Assert.Equal(1, service.LastRequest.Adultos);
		Assert.Equal("ECONOMY", service.LastRequest.Classe);
		Assert.Equal(10, service.LastRequest.MaxResultados);
		Assert.Equal(0, service.LastRequest.Criancas);
		Assert.Equal(0, service.LastRequest.Bebes);
		Assert.Equal("BRL", service.LastRequest.Moeda);
	}

	[Fact]
	public async Task SearchGet_Maps_Provided_Values_Including_DataVolta()
	{
		var service = new CapturingFrontendService();
		var sut = new FrontendTravelTicketsController(service);
		var query = new FrontendTravelTicketQueryRequest
		{
			Origem = "GRU",
			Destino = "JFK",
			DataIda = new DateOnly(2026, 5, 10),
			DataVolta = new DateOnly(2026, 5, 20),
			Adultos = 2,
			Classe = "BUSINESS",
			MaxResultados = 6
		};

		await sut.SearchGet(query, CancellationToken.None);

		Assert.NotNull(service.LastRequest);
		Assert.Equal("GRU", service.LastRequest!.Origem);
		Assert.Equal(new DateOnly(2026, 5, 20), service.LastRequest.DataVolta);
		Assert.Equal(2, service.LastRequest.Adultos);
		Assert.Equal("BUSINESS", service.LastRequest.Classe);
		Assert.Equal(6, service.LastRequest.MaxResultados);
	}

	[Fact]
	public void SearchGet_Has_FromQuery_And_No_FromBody()
	{
		var method = typeof(FrontendTravelTicketsController).GetMethod(nameof(FrontendTravelTicketsController.SearchGet));
		Assert.NotNull(method);
		var parameter = method!.GetParameters().First(p => p.ParameterType == typeof(FrontendTravelTicketQueryRequest));
		Assert.NotNull(parameter.GetCustomAttributes(typeof(FromQueryAttribute), false).SingleOrDefault());
		Assert.Empty(parameter.GetCustomAttributes(typeof(FromBodyAttribute), false));
	}

	private sealed class CapturingFrontendService : IFrontendTravelTicketService
	{
		public FrontendTravelTicketSearchRequest? LastRequest { get; private set; }
		public IReadOnlyList<FrontendTravelTicketDto> Response { get; } =
		[
			new FrontendTravelTicketDto
			{
				Id = 1,
				CiaAerea = "LATAM",
				HoraPartidaIda = "08:30",
				AeroPartidaIda = "GRU",
				DataPartidaIda = "2026-05-10",
				HoraChegadaIda = "12:00",
				AeroChegadaIda = "JFK",
				DataChegadaIda = "2026-05-10",
				Paradas = 0,
				Valor = 3500m
			}
		];

		public Task<IReadOnlyList<FrontendTravelTicketDto>> SearchAsync(FrontendTravelTicketSearchRequest request,
			CancellationToken cancellationToken)
		{
			LastRequest = request;
			return Task.FromResult(Response);
		}
	}
}
