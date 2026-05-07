using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Infrastructure.Services.Airports;

public sealed class OurAirportsSyncHostedService(
	IServiceScopeFactory scopeFactory,
	IOptions<AirportDataSyncOptions> options,
	ILogger<OurAirportsSyncHostedService> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var opts = options.Value;
		if (!opts.Enabled)
		{
			logger.LogInformation("Airport data sync hosted service is disabled.");
			return;
		}

		if (opts.SyncOnStartup)
		{
			var delay = TimeSpan.FromSeconds(Math.Max(0, opts.StartupDelaySeconds));
			if (delay > TimeSpan.Zero)
				await Task.Delay(delay, stoppingToken);

			await RunSyncAsync(force: false, stoppingToken);
		}

		var period = TimeSpan.FromHours(Math.Max(1, opts.PeriodicCheckHours));
		using var timer = new PeriodicTimer(period);
		while (await timer.WaitForNextTickAsync(stoppingToken))
			await RunSyncAsync(force: false, stoppingToken);
	}

	private async Task RunSyncAsync(bool force, CancellationToken cancellationToken)
	{
		try
		{
			using var scope = scopeFactory.CreateScope();
			var sync = scope.ServiceProvider.GetRequiredService<IAirportDataSyncService>();
			await sync.SyncIfNeededAsync(force, cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Airport data sync hosted service failed.");
		}
	}
}
