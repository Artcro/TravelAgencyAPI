namespace TravelAgency.Infrastructure.Services.Airports;

public sealed record AirportDataSyncResult(bool Synced, bool Success, int ImportedCount, string Reason);
