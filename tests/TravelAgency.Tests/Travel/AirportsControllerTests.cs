using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TravelAgency.Api.Controllers;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;
using TravelAgency.Infrastructure.Services.Airports;

namespace TravelAgency.Tests.Travel;

public sealed class AirportsControllerTests
{
	[Fact]
	public async Task Search_Returns_Raw_Airport_Array_With_Portuguese_Field_Contract()
	{
		var db = CreateDb();
		SeedAirport(db, "GRU", "São Paulo", "BR", "Brasil", "Aeroporto Internacional de Guarulhos");
		await db.SaveChangesAsync();

		var sut = new AirportsController(db, NullLogger<AirportsController>.Instance);
		var result = await sut.Search("sao", null, CancellationToken.None);

		var ok = Assert.IsType<OkObjectResult>(result);
		var items = Assert.IsAssignableFrom<IReadOnlyList<AirportAutocompleteDto>>(ok.Value);
		var item = Assert.Single(items);
		Assert.Equal("GRU", item.Iata);
		Assert.Equal("São Paulo", item.Cidade);
		Assert.Equal("Brasil", item.Pais);
		Assert.Equal("Aeroporto Internacional de Guarulhos", item.Nome);
	}

	[Fact]
	public async Task Search_Prioritizes_Exact_Iata_Match()
	{
		var db = CreateDb();
		SeedAirport(db, "GRU", "São Paulo", "BR", "Brasil", "Aeroporto Internacional de Guarulhos");
		SeedAirport(db, "SAO", "São Paulo", "BR", "Brasil", "Metropolitan Area");
		await db.SaveChangesAsync();

		var sut = new AirportsController(db, NullLogger<AirportsController>.Instance);
		var result = await sut.Search("sao", 8, CancellationToken.None);

		var ok = Assert.IsType<OkObjectResult>(result);
		var items = Assert.IsAssignableFrom<IReadOnlyList<AirportAutocompleteDto>>(ok.Value);
		Assert.Equal("SAO", items[0].Iata);
	}

	[Fact]
	public async Task Search_Invalid_Query_Returns_Contract_Error()
	{
		var sut = new AirportsController(CreateDb(), NullLogger<AirportsController>.Instance);

		var result = await sut.Search("a", null, CancellationToken.None);

		var badRequest = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Equal("{ error = q must be at least 2 characters }", badRequest.Value!.ToString());
	}

	private static TravelDbContext CreateDb()
	{
		return new TravelDbContext(new DbContextOptionsBuilder<TravelDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options);
	}

	private static void SeedAirport(TravelDbContext db, string iata, string city, string countryCode,
		string countryName, string name)
	{
		db.Airports.Add(new AirportEntity
		{
			IataCode = iata,
			Ident = iata,
			Name = name,
			City = city,
			CountryCode = countryCode,
			CountryName = countryName,
			CitySearch = AirportTextNormalizer.Normalize(city),
			NameSearch = AirportTextNormalizer.Normalize(name),
			CountrySearch = AirportTextNormalizer.Normalize(countryName),
			AirportType = "large_airport",
			ScheduledService = true,
			IsActive = true,
			LastSyncedAtUtc = DateTime.UtcNow
		});
	}
}
