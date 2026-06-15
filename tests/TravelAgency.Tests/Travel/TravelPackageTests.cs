using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Api.Controllers;
using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;
using TravelAgency.Domain.Trips;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Travel;

namespace TravelAgency.Tests.Travel;

public sealed class TravelPackageServiceTests
{
	private static TravelDbContext CreateDb()
	{
		return new TravelDbContext(new DbContextOptionsBuilder<TravelDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options);
	}

	private static SaveTravelPackageRequest ValidRequest()
	{
		return new SaveTravelPackageRequest
		{
			Destino = "Roma",
			DataViagem = "2026-05-10",
			Viajantes = 2,
			CiaAerea = "LATAM",
			Hotel = "Roma Grand Hotel",
			HotelValor = 520m,
			Carro = "Fiat Mobi",
			CarroValor = 89.9m,
			Passeio = "City tour",
			PasseioValor = 120m,
			ValorVoo = 3000m
		};
	}

	[Fact]
	public async Task Save_Computes_Total_And_Persists_For_User()
	{
		using var db = CreateDb();
		var service = new TravelPackageService(db);
		var userId = Guid.NewGuid();

		var result = await service.SaveAsync(userId, ValidRequest(), CancellationToken.None);

		Assert.True(result.IsValid);
		Assert.Equal(3729.9m, result.Value!.Valor);
		Assert.Equal(PackageStatus.Confirmed, result.Value.Status);
		Assert.Equal(1, await db.TravelPackages.CountAsync());
	}

	[Fact]
	public async Task Save_Invalid_Returns_Errors_And_Persists_Nothing()
	{
		using var db = CreateDb();
		var service = new TravelPackageService(db);

		var result = await service.SaveAsync(Guid.NewGuid(),
			new SaveTravelPackageRequest { Destino = "", Viajantes = 0 }, CancellationToken.None);

		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.Field == "destino");
		Assert.Contains(result.Errors, e => e.Field == "viajantes");
		Assert.Equal(0, await db.TravelPackages.CountAsync());
	}

	[Fact]
	public async Task List_Only_Returns_Current_User_Packages()
	{
		using var db = CreateDb();
		var service = new TravelPackageService(db);
		var userA = Guid.NewGuid();
		var userB = Guid.NewGuid();

		await service.SaveAsync(userA, ValidRequest(), CancellationToken.None);
		await service.SaveAsync(userB, ValidRequest(), CancellationToken.None);

		var list = await service.ListAsync(userA, CancellationToken.None);

		Assert.Single(list);
	}

	[Fact]
	public async Task Cancel_Sets_Status_To_Cancelled()
	{
		using var db = CreateDb();
		var service = new TravelPackageService(db);
		var userId = Guid.NewGuid();
		var saved = await service.SaveAsync(userId, ValidRequest(), CancellationToken.None);

		var cancelled = await service.CancelAsync(userId, saved.Value!.Id, CancellationToken.None);

		Assert.NotNull(cancelled);
		Assert.Equal(PackageStatus.Cancelled, cancelled!.Status);
	}

	[Fact]
	public async Task Cancel_Other_Users_Package_Returns_Null()
	{
		using var db = CreateDb();
		var service = new TravelPackageService(db);
		var saved = await service.SaveAsync(Guid.NewGuid(), ValidRequest(), CancellationToken.None);

		var cancelled = await service.CancelAsync(Guid.NewGuid(), saved.Value!.Id, CancellationToken.None);

		Assert.Null(cancelled);
	}

	[Fact]
	public async Task Save_Drops_Nenhum_Optional_Values()
	{
		using var db = CreateDb();
		var service = new TravelPackageService(db);
		var request = ValidRequest();
		request.Carro = "nenhum";
		request.CarroValor = null;

		var result = await service.SaveAsync(Guid.NewGuid(), request, CancellationToken.None);

		Assert.Null(result.Value!.Carro);
	}
}

public sealed class TravelPackagesControllerTests
{
	[Fact]
	public async Task Save_Without_User_Returns_Unauthorized()
	{
		var controller = new TravelPackagesController(new StubPackageService(), new StubUser(null));

		var result = await controller.Save(new SaveTravelPackageRequest(), CancellationToken.None);

		Assert.IsType<UnauthorizedResult>(result);
	}

	[Fact]
	public async Task List_With_User_Returns_Ok()
	{
		var controller = new TravelPackagesController(new StubPackageService(), new StubUser(Guid.NewGuid()));

		var result = await controller.List(CancellationToken.None);

		Assert.IsType<OkObjectResult>(result);
	}

	private sealed class StubPackageService : ITravelPackageService
	{
		public Task<Result<TravelPackageDto>> SaveAsync(Guid userId, SaveTravelPackageRequest request,
			CancellationToken cancellationToken)
		{
			return Task.FromResult(Result<TravelPackageDto>.Ok(new TravelPackageDto()));
		}

		public Task<IReadOnlyList<TravelPackageDto>> ListAsync(Guid userId, CancellationToken cancellationToken)
		{
			return Task.FromResult<IReadOnlyList<TravelPackageDto>>([]);
		}

		public Task<TravelPackageDto?> CancelAsync(Guid userId, Guid packageId, CancellationToken cancellationToken)
		{
			return Task.FromResult<TravelPackageDto?>(null);
		}

		public Task<bool> DeleteAsync(Guid userId, Guid packageId, CancellationToken cancellationToken)
		{
			return Task.FromResult(false);
		}
	}

	private sealed class StubUser(Guid? userId) : ICurrentUserService
	{
		public Guid? UserId { get; } = userId;
		public string? Email => null;
		public bool IsAuthenticated => UserId is not null;
	}
}
