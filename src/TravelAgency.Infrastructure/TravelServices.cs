using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure;

public sealed class TripSearchService(TripSearchRequestValidator validator, TripResultNormalizer normalizer, IFlightProvider flightProvider, IHotelProvider hotelProvider, IActivityProvider activityProvider, TravelDbContext db, ILogger<TripSearchService> logger) : ITripSearchService
{
    public async Task<TripSearchResponse> SearchAsync(TripSearchRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        request.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BRL" : request.Currency;
        request.TravelClass = string.IsNullOrWhiteSpace(request.TravelClass) ? "ECONOMY" : request.TravelClass;
        var errors = validator.Validate(request);
        if (errors.Count > 0) throw new ArgumentException(string.Join(" ", errors));
        List<string> warnings = [];
        IReadOnlyList<FlightOptionDto> flights;
        try { flights = await flightProvider.SearchFlightsAsync(request, cancellationToken); }
        catch (Exception ex) { throw new InvalidOperationException("Flight provider failed.", ex); }
        var hotels = new List<HotelOptionDto>(); var acts = new List<ActivityOptionDto>();
        if (request.IncludeHotels) { try { hotels = (await hotelProvider.SearchHotelsAsync(request, cancellationToken)).ToList(); warnings.Add("Hotel results are mocked."); } catch { warnings.Add("Hotel provider unavailable."); } }
        if (request.IncludeActivities) { try { acts = (await activityProvider.SearchActivitiesAsync(request, cancellationToken)).ToList(); warnings.Add("Activity results are mocked."); } catch { warnings.Add("Activity provider unavailable."); } }
        var response = normalizer.Normalize(request, flights, hotels, acts, warnings);
        response.SearchId = Guid.NewGuid();
        db.TripSearches.Add(new TripSearchEntity { Id = response.SearchId, UserId = userId, Origin = request.Origin, Destination = request.Destination, DepartureDate = request.DepartureDate, ReturnDate = request.ReturnDate, Adults = request.Adults, Children = request.Children, Infants = request.Infants, Currency = request.Currency, RequestJson = JsonSerializer.Serialize(request), ResponseJson = JsonSerializer.Serialize(response), CreatedAtUtc = DateTime.UtcNow, ProviderStatus = warnings.Any(x => x.Contains("unavailable")) ? "Partial" : "Completed" });
        await db.SaveChangesAsync(cancellationToken); return response;
    }
    public async Task<TripSearchResponse?> GetSearchByIdAsync(Guid searchId, Guid? userId, CancellationToken cancellationToken)
    {
        var e = await db.TripSearches.FirstOrDefaultAsync(x => x.Id == searchId, cancellationToken);
        return e is null ? null : JsonSerializer.Deserialize<TripSearchResponse>(e.ResponseJson);
    }
}

public sealed class SavedTripService(TravelDbContext db, IOptions<SecurityOptions> security, ILogger<SavedTripService> logger) : ISavedTripService
{
    public async Task<SavedTripResponse> SaveAsync(SaveTripRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var entity = new SavedTripEntity { Id = Guid.NewGuid(), UserId = security.Value.RequireAuthentication ? userId : null, SearchId = request.SearchId, Name = request.Name, SelectedFlightProviderOfferId = request.SelectedFlightProviderOfferId, SelectedHotelProviderHotelId = request.SelectedHotelProviderHotelId, SelectedActivityIdsJson = JsonSerializer.Serialize(request.SelectedActivityIds), CreatedAtUtc = DateTime.UtcNow };
        db.SavedTrips.Add(entity); db.AuditLogs.Add(new AuditLogEntity { Id = Guid.NewGuid(), UserId = entity.UserId, Action = "trip_saved", ResourceType = "SavedTrip", ResourceId = entity.Id.ToString(), CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync(cancellationToken); return new SavedTripResponse(entity.Id, entity.Status);
    }
    public async Task<IReadOnlyList<object>> ListAsync(Guid? userId, CancellationToken cancellationToken) => await db.SavedTrips.Where(x => !x.IsDeleted).Select(x => (object)new { x.Id, x.Name, x.Status, x.CreatedAtUtc }).ToListAsync(cancellationToken);
    public async Task<object?> GetByIdAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken) => await db.SavedTrips.Where(x => x.Id == tripId && !x.IsDeleted).Select(x => (object)new { x.Id, x.Name, x.Status }).FirstOrDefaultAsync(cancellationToken);
    public async Task<bool> DeleteAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken) { var e = await db.SavedTrips.FirstOrDefaultAsync(x => x.Id == tripId && !x.IsDeleted, cancellationToken); if (e is null) return false; e.IsDeleted = true; e.UpdatedAtUtc = DateTime.UtcNow; db.AuditLogs.Add(new AuditLogEntity { Id = Guid.NewGuid(), UserId = e.UserId, Action = "saved_trip_deleted", ResourceType = "SavedTrip", ResourceId = e.Id.ToString(), CreatedAtUtc = DateTime.UtcNow }); await db.SaveChangesAsync(cancellationToken); return true; }
}

public sealed class SecurityOptions { public const string SectionName = "Security"; public bool RequireAuthentication { get; set; } }
