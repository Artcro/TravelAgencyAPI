using TravelAgency.Domain.Trips;

namespace TravelAgency.Infrastructure.Database.Entities;

public sealed class SavedTripEntity
{
	public Guid Id { get; set; }
	public Guid? UserId { get; set; }
	public ApplicationUser? User { get; set; }
	public Guid SearchId { get; set; }
	public TripSearchEntity Search { get; set; } = default!;
	public string Name { get; set; } = string.Empty;
	public string? SelectedFlightProviderOfferId { get; set; }
	public string? SelectedHotelProviderHotelId { get; set; }
	public string SelectedActivityIdsJson { get; set; } = "[]";
	public string Status { get; set; } = SavedTripStatus.Saved;
	public bool IsDeleted { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public DateTime? UpdatedAtUtc { get; set; }
}