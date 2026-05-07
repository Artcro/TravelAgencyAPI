using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Options;
using TravelAgency.Infrastructure.Providers.Local;
using TravelAgency.Infrastructure.Services.Airports;

namespace TravelAgency.Tests.Travel;

public sealed class AirportCacheTests
{
	[Fact]
	public async Task OurAirportsSync_Imports_Iata_Airports_And_LocalProvider_Searches_Them()
	{
		var db = CreateDb();
		var service = new OurAirportsDataSyncService(
			new StubFactory(new HttpClient(new CsvHandler())),
			Options.Create(new AirportDataSyncOptions
			{
				AirportsCsvUrl = "https://example.test/airports.csv",
				CountriesCsvUrl = "https://example.test/countries.csv",
				MinimumAirportCount = 0
			}),
			db,
			NullLogger<OurAirportsDataSyncService>.Instance);

		var result = await service.SyncIfNeededAsync(force: true, default);

		Assert.True(result.Success);
		Assert.Equal(2, result.ImportedCount);
		Assert.Equal(2, await db.Airports.CountAsync());
		Assert.DoesNotContain(db.Airports, x => x.IataCode == "ZZZ");

		var provider = new LocalAirportLocationProvider(db);
		var matches = await provider.SearchLocationsAsync("sao", default);

		var gru = Assert.Single(matches.Where(x => x.Code == "GRU"));
		Assert.Equal("Sao Paulo", gru.City);
		Assert.Equal("Brasil", gru.CountryName);
	}

	[Fact]
	public async Task LocalProvider_Prioritizes_Exact_Iata_Code()
	{
		var db = CreateDb();
		db.Airports.AddRange(
			new()
			{
				IataCode = "GRU",
				Name = "Sao Paulo Guarulhos International Airport",
				City = "Sao Paulo",
				CountryCode = "BR",
				CountryName = "Brazil",
				AirportType = "large_airport",
				ScheduledService = true,
				IsActive = true,
				LastSyncedAtUtc = DateTime.UtcNow
			},
			new()
			{
				IataCode = "RIO",
				Name = "Rio de Janeiro Airport",
				City = "Rio de Janeiro",
				CountryCode = "BR",
				CountryName = "Brazil",
				AirportType = "medium_airport",
				ScheduledService = true,
				IsActive = true,
				LastSyncedAtUtc = DateTime.UtcNow
			});
		await db.SaveChangesAsync();

		var provider = new LocalAirportLocationProvider(db);
		var matches = await provider.SearchLocationsAsync("rio", default);

		Assert.Equal("RIO", matches[0].Code);
	}

	private static TravelDbContext CreateDb()
	{
		return new TravelDbContext(new DbContextOptionsBuilder<TravelDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options);
	}

	private sealed class StubFactory(HttpClient client) : IHttpClientFactory
	{
		public HttpClient CreateClient(string name)
		{
			return client;
		}
	}

	private sealed class CsvHandler : HttpMessageHandler
	{
		private const string AirportsCsv =
			"id,ident,type,name,latitude_deg,longitude_deg,elevation_ft,continent,iso_country,iso_region,municipality,scheduled_service,gps_code,icao_code,iata_code,local_code,home_link,wikipedia_link,keywords\n" +
			"1,SBGR,large_airport,Sao Paulo Guarulhos International Airport,-23.4356,-46.4731,2459,SA,BR,BR-SP,Sao Paulo,yes,SBGR,SBGR,GRU,,,,\n" +
			"2,SBRJ,medium_airport,Rio de Janeiro Santos Dumont Airport,-22.91,-43.16,11,SA,BR,BR-RJ,Rio de Janeiro,yes,SBRJ,SBRJ,SDU,,,,\n" +
			"3,ZZZZ,closed_airport,Closed Test Airport,0,0,,NA,US,US-CA,Nowhere,no,,,ZZZ,,,,\n";

		private const string CountriesCsv =
			"id,code,name,continent,wikipedia_link,keywords\n" +
			"1,BR,Brazil,SA,,\n" +
			"2,US,United States,NA,,\n";

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			var body = request.RequestUri!.AbsolutePath.Contains("countries", StringComparison.OrdinalIgnoreCase)
				? CountriesCsv
				: AirportsCsv;

			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
				{ Content = new StringContent(body, Encoding.UTF8, "text/csv") });
		}
	}
}
