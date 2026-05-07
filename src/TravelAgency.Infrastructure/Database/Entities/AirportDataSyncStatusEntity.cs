namespace TravelAgency.Infrastructure.Database.Entities;

public sealed class AirportDataSyncStatusEntity
{
	public string Source { get; set; } = "";
	public DateTime? LastSucceededAtUtc { get; set; }
	public DateTime? LastAttemptedAtUtc { get; set; }
	public int ImportedAirportCount { get; set; }
	public int SourceRecordCount { get; set; }
	public string? ErrorMessage { get; set; }
}
