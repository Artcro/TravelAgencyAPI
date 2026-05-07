namespace TravelAgency.Infrastructure.Services.Airports;

public interface IAirportDataSyncService
{
	Task<AirportDataSyncResult> SyncIfNeededAsync(bool force, CancellationToken cancellationToken);
}
