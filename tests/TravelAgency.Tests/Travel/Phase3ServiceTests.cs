using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TravelAgency.Application.Common;
using TravelAgency.Application.Config;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;
using TravelAgency.Infrastructure.Travel;

namespace TravelAgency.Tests.Travel;

public class Phase3ServiceTests
{
	[Fact]
	public async Task TripSearch_Persists_And_Warnings_For_Mocks()
	{
		var db = CreateDb();
		var svc = new TripSearchService(new TripSearchRequestValidator(), new TripResultNormalizer(),
			new OkFlightProvider(), new OkHotelProvider(), new OkActivityProvider(), db,
			NullLogger<TripSearchService>.Instance);

		var req = ValidRequest();
		var result = await svc.SearchAsync(req, null, default);
		Assert.True(result.IsValid);
		Assert.NotNull(result.Value);
		Assert.Contains(result.Value!.Warnings, x => x.Contains("mocked"));
		Assert.NotNull(await db.TripSearches.FirstOrDefaultAsync(x => x.Id == result.Value!.SearchId));
	}

	[Fact]
	public async Task TripSearch_Returns_Flights_If_Hotel_Fails()
	{
		var db = CreateDb();
		var svc = new TripSearchService(new TripSearchRequestValidator(), new TripResultNormalizer(),
			new OkFlightProvider(), new FailingHotelProvider(), new OkActivityProvider(), db,
			NullLogger<TripSearchService>.Instance);

		var result = await svc.SearchAsync(ValidRequest(), null, default);
		Assert.True(result.IsValid);
		Assert.NotEmpty(result.Value!.Flights);
		Assert.Contains(result.Value!.Warnings, x => x.Contains("Hotel provider unavailable"));
	}

	[Fact]
	public async Task TripSearch_Fails_If_Flight_Fails()
	{
		var db = CreateDb();
		var svc = new TripSearchService(new TripSearchRequestValidator(), new TripResultNormalizer(),
			new FailingFlightProvider(), new OkHotelProvider(), new OkActivityProvider(), db,
			NullLogger<TripSearchService>.Instance);

		await Assert.ThrowsAsync<InvalidOperationException>(() => svc.SearchAsync(ValidRequest(), null, default));
	}

	[Fact]
	public async Task SavedTrip_Anonymous_Allowed_When_Disabled()
	{
		var db = CreateDb();
		var saved = new SavedTripService(db, Options.Create(new SecurityOptions { RequireAuthentication = false }),
			new FakeCurrentUser(null, false), NullLogger<SavedTripService>.Instance);

		var searchId = await SeedSearch(db);
		await saved.SaveAsync(new SaveTripRequest { SearchId = searchId, Name = "demo" }, null, default);
		Assert.Null(db.SavedTrips.Single().UserId);
	}

	[Fact]
	public async Task SavedTrip_Sets_User_When_Required()
	{
		var db = CreateDb();
		var uid = Guid.NewGuid();
		var saved = new SavedTripService(db, Options.Create(new SecurityOptions { RequireAuthentication = true }),
			new FakeCurrentUser(uid, true), NullLogger<SavedTripService>.Instance);

		var searchId = await SeedSearch(db);
		await saved.SaveAsync(new SaveTripRequest { SearchId = searchId, Name = "mine" }, null, default);
		Assert.Equal(uid, db.SavedTrips.Single().UserId);
	}

	[Fact]
	public async Task SavedTrip_Blocks_Other_User_And_SoftDeletes()
	{
		var db = CreateDb();
		var owner = Guid.NewGuid();
		var trip = await SeedSavedTrip(db, owner);
		var saved = new SavedTripService(db, Options.Create(new SecurityOptions { RequireAuthentication = true }),
			new FakeCurrentUser(Guid.NewGuid(), true), NullLogger<SavedTripService>.Instance);

		Assert.Null(await saved.GetByIdAsync(trip, null, default));

		var ownerSvc = new SavedTripService(db, Options.Create(new SecurityOptions { RequireAuthentication = true }),
			new FakeCurrentUser(owner, true), NullLogger<SavedTripService>.Instance);

		Assert.True(await ownerSvc.DeleteAsync(trip, null, default));
		Assert.True(db.SavedTrips.Single().IsDeleted);
	}

	[Fact]
	public void Validator_Rejects_And_Defaults_Work()
	{
		var v = new TripSearchRequestValidator();
		var req = new TripSearchRequest
		{
			Origin = "JFK", Destination = "JFK", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
			ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), Adults = 0, MaxFlightResults = 100
		};

		IReadOnlyList<ValidationError> errors = v.Validate(req);
		Assert.True(errors.Count >= 5);
	}

	private static TravelDbContext CreateDb()
	{
		return new TravelDbContext(
			new DbContextOptionsBuilder<TravelDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
	}

	private static TripSearchRequest ValidRequest()
	{
		return new TripSearchRequest
		{
			Origin = "JFK", Destination = "LAX", DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
			Adults = 1, IncludeActivities = true, IncludeHotels = true, MaxFlightResults = 5, Currency = "USD"
		};
	}

	private static async Task<Guid> SeedSearch(TravelDbContext db)
	{
		var id = Guid.NewGuid();
		db.TripSearches.Add(new TripSearchEntity
		{
			Id = id, Origin = "JFK", Destination = "LAX",
			DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)), Adults = 1, Currency = "USD",
			RequestJson = "{}", ResponseJson = "{}", CreatedAtUtc = DateTime.UtcNow, ProviderStatus = "Completed"
		});

		await db.SaveChangesAsync();
		return id;
	}

	private static async Task<Guid> SeedSavedTrip(TravelDbContext db, Guid uid)
	{
		var sid = await SeedSearch(db);
		var id = Guid.NewGuid();
		db.SavedTrips.Add(new SavedTripEntity
			{ Id = id, SearchId = sid, UserId = uid, Name = "x", CreatedAtUtc = DateTime.UtcNow });

		await db.SaveChangesAsync();
		return id;
	}

	private sealed record FakeCurrentUser(Guid? UserId, bool IsAuthenticated) : ICurrentUserService
	{
		public string? Email => null;
	}

	private sealed class OkFlightProvider : IFlightProvider
	{
		public Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request,
			CancellationToken cancellationToken)
		{
			return Task.FromResult<IReadOnlyList<FlightOptionDto>>([
				new FlightOptionDto
				{
					Provider = "x", ProviderOfferId = "1", AirlineCode = "AA", TotalPrice = new MoneyDto(1, "USD"),
					Duration = "PT1H", Stops = 0
				}
			]);
		}
	}

	private sealed class FailingFlightProvider : IFlightProvider
	{
		public Task<IReadOnlyList<FlightOptionDto>> SearchFlightsAsync(TripSearchRequest request,
			CancellationToken cancellationToken)
		{
			throw new Exception("boom");
		}
	}

	private sealed class OkHotelProvider : IHotelProvider
	{
		public Task<IReadOnlyList<HotelOptionDto>> SearchHotelsAsync(TripSearchRequest request,
			CancellationToken cancellationToken)
		{
			return Task.FromResult<IReadOnlyList<HotelOptionDto>>([]);
		}
	}

	private sealed class FailingHotelProvider : IHotelProvider
	{
		public Task<IReadOnlyList<HotelOptionDto>> SearchHotelsAsync(TripSearchRequest request,
			CancellationToken cancellationToken)
		{
			throw new Exception("boom");
		}
	}

	private sealed class OkActivityProvider : IActivityProvider
	{
		public Task<IReadOnlyList<ActivityOptionDto>> SearchActivitiesAsync(TripSearchRequest request,
			CancellationToken cancellationToken)
		{
			return Task.FromResult<IReadOnlyList<ActivityOptionDto>>([]);
		}
	}
}