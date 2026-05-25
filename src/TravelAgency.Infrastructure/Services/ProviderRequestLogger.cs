using Microsoft.Extensions.Logging;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure.Services;

/// <summary>
/// Best-effort persistence of provider HTTP call metrics into
/// <c>ProviderRequestLogs</c>. Owns the try/catch+log-and-swallow contract so
/// individual providers don't reimplement it.
/// </summary>
public sealed class ProviderRequestLogger(TravelDbContext db, ILogger<ProviderRequestLogger> logger)
{
	public async Task RecordAsync(string provider, string endpoint, int statusCode, bool success, long durationMs,
		string? errorMessage = null, CancellationToken cancellationToken = default)
	{
		try
		{
			db.ProviderRequestLogs.Add(new ProviderRequestLogEntity
			{
				Id = Guid.NewGuid(),
				Provider = provider,
				Endpoint = endpoint,
				StatusCode = statusCode,
				Success = success,
				ErrorMessage = errorMessage,
				DurationMs = durationMs,
				CreatedAtUtc = DateTime.UtcNow
			});

			await db.SaveChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex,
				"Best-effort provider request logging failed for {Provider} {Endpoint}.", provider, endpoint);
		}
	}
}
