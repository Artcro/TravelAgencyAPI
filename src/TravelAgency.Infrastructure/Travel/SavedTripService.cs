using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TravelAgency.Application.Config;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;
using TravelAgency.Domain.Trips;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure.Travel;

public sealed class SavedTripService(
	TravelDbContext db,
	IOptions<SecurityOptions> security,
	ICurrentUserService currentUserService) : ISavedTripService
{
	public async Task<SavedTripResponse> SaveAsync(SaveTripRequest request, Guid? userId,
		CancellationToken cancellationToken)
	{
		var effectiveUserId = ResolveUserId(userId);
		var entity = new SavedTripEntity
		{
			Id = Guid.NewGuid(), UserId = effectiveUserId, SearchId = request.SearchId, Name = request.Name,
			SelectedFlightProviderOfferId = request.SelectedFlightProviderOfferId,
			SelectedHotelProviderHotelId = request.SelectedHotelProviderHotelId,
			SelectedActivityIdsJson = JsonSerializer.Serialize(request.SelectedActivityIds),
			CreatedAtUtc = DateTime.UtcNow
		};

		db.SavedTrips.Add(entity);
		db.AuditLogs.Add(new AuditLogEntity
		{
			Id = Guid.NewGuid(), UserId = entity.UserId, Action = AuditAction.TripSaved, ResourceType = "SavedTrip",
			ResourceId = entity.Id.ToString(), CreatedAtUtc = DateTime.UtcNow
		});

		await db.SaveChangesAsync(cancellationToken);
		return new SavedTripResponse(entity.Id, entity.Status);
	}

	public async Task<IReadOnlyList<SavedTripSummaryDto>> ListAsync(Guid? userId, CancellationToken cancellationToken)
	{
		var effectiveUserId = ResolveUserId(userId);
		var query = db.SavedTrips.Where(x => !x.IsDeleted);
		if (security.Value.RequireAuthentication) query = query.Where(x => x.UserId == effectiveUserId);

		return await query.OrderByDescending(x => x.CreatedAtUtc)
			.Select(x => new SavedTripSummaryDto(x.Id, x.Name, x.Status, x.CreatedAtUtc))
			.ToListAsync(cancellationToken);
	}

	public async Task<SavedTripDetailDto?> GetByIdAsync(Guid tripId, Guid? userId,
		CancellationToken cancellationToken)
	{
		var effectiveUserId = ResolveUserId(userId);
		var query = db.SavedTrips.Where(x => x.Id == tripId && !x.IsDeleted);
		if (security.Value.RequireAuthentication) query = query.Where(x => x.UserId == effectiveUserId);

		return await query.Select(x => new SavedTripDetailDto(x.Id, x.Name, x.Status, x.CreatedAtUtc, x.SearchId,
				x.UserId, x.SelectedFlightProviderOfferId, x.SelectedHotelProviderHotelId))
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<bool> DeleteAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken)
	{
		var effectiveUserId = ResolveUserId(userId);
		var query = db.SavedTrips.Where(x => x.Id == tripId && !x.IsDeleted);
		if (security.Value.RequireAuthentication) query = query.Where(x => x.UserId == effectiveUserId);

		var e = await query.FirstOrDefaultAsync(cancellationToken);
		if (e is null) return false;

		e.IsDeleted = true;
		e.UpdatedAtUtc = DateTime.UtcNow;
		db.AuditLogs.Add(new AuditLogEntity
		{
			Id = Guid.NewGuid(), UserId = e.UserId, Action = AuditAction.SavedTripDeleted,
			ResourceType = "SavedTrip", ResourceId = e.Id.ToString(), CreatedAtUtc = DateTime.UtcNow
		});

		await db.SaveChangesAsync(cancellationToken);
		return true;
	}

	private Guid? ResolveUserId(Guid? providedUserId)
	{
		if (security.Value.RequireAuthentication)
		{
			if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
				throw new UnauthorizedAccessException("Authentication is required.");

			return currentUserService.UserId;
		}

		return providedUserId ?? currentUserService.UserId;
	}
}
