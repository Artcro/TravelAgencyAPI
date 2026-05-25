namespace TravelAgency.Domain.Trips;

/// <summary>Names for audit-log <c>Action</c> values written by the trips domain.</summary>
public static class AuditAction
{
	public const string TripSaved = "trip_saved";
	public const string SavedTripDeleted = "saved_trip_deleted";
}
