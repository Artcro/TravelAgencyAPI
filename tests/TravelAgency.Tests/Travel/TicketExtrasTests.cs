using Microsoft.AspNetCore.Mvc;
using TravelAgency.Api.Controllers;
using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Providers.Mock;
using TravelAgency.Infrastructure.Travel;

namespace TravelAgency.Tests.Travel;

public sealed class TicketExtrasServiceTests
{
	private static TicketExtrasService CreateService()
	{
		return new TicketExtrasService(new MockHotelProvider(), new MockActivityProvider(), new MockCarRentalProvider());
	}

	[Fact]
	public async Task GetExtras_Returns_Hotels_Activities_And_Cars()
	{
		var result = await CreateService().GetExtrasAsync("GIG", null, CancellationToken.None);

		Assert.True(result.IsValid);
		Assert.NotNull(result.Value);
		Assert.NotEmpty(result.Value!.Hoteis);
		Assert.NotEmpty(result.Value.Passeios);
		Assert.NotEmpty(result.Value.Carros);

		var hotel = result.Value.Hoteis[0];
		Assert.False(string.IsNullOrWhiteSpace(hotel.Id));
		Assert.False(string.IsNullOrWhiteSpace(hotel.Nome));
		Assert.True(hotel.PrecoPorNoite > 0);

		var carro = result.Value.Carros[0];
		Assert.Equal("Fiat Mobi", carro.Modelo);
		Assert.True(carro.PrecoPorDia > 0);
	}

	[Fact]
	public async Task GetExtras_Invalid_Destino_Returns_Validation_Error()
	{
		var result = await CreateService().GetExtrasAsync("XX", null, CancellationToken.None);

		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Field == "destino");
	}
}

public sealed class TicketExtrasControllerTests
{
	[Fact]
	public async Task Extras_Missing_Destino_Returns_BadRequest()
	{
		var controller = new TicketExtrasController(new StubExtrasService());

		var result = await controller.Extras(new TicketExtrasQueryRequest { Destino = "" }, CancellationToken.None);

		Assert.IsType<BadRequestObjectResult>(result.Result);
	}

	[Fact]
	public async Task Extras_Valid_Destino_Returns_Ok_With_Extras()
	{
		var stub = new StubExtrasService();
		var controller = new TicketExtrasController(stub);

		var result = await controller.Extras(new TicketExtrasQueryRequest { Destino = "GIG" }, CancellationToken.None);

		var ok = Assert.IsType<OkObjectResult>(result.Result);
		Assert.Same(stub.Response, ok.Value);
		Assert.Equal("GIG", stub.LastDestino);
	}

	private sealed class StubExtrasService : ITicketExtrasService
	{
		public string? LastDestino { get; private set; }
		public TicketExtrasDto Response { get; } = new();

		public Task<Result<TicketExtrasDto>> GetExtrasAsync(string destino, string? moeda,
			CancellationToken cancellationToken)
		{
			LastDestino = destino;
			return Task.FromResult(Result<TicketExtrasDto>.Ok(Response));
		}
	}
}
