using Microsoft.EntityFrameworkCore;
using TravelAgency.Infrastructure.Database;

namespace TravelAgency.Api.Config;

internal static class MigrationStartupExtensions
{
	private static readonly string[] RequiredTables =
	[
		"ProviderRequestLogs", "TripSearches", "SavedTrips", "AuditLogs", "Airports", "AirportDataSyncStatuses"
	];

	public static async Task ApplyMigrationsAndVerifyAsync(this WebApplication app)
	{
		var applyMigrationsOnStartup = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
		if (!applyMigrationsOnStartup) return;

		using var scope = app.Services.CreateScope();
		var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigrations");
		try
		{
			var db = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
			await db.Database.MigrateAsync();
			logger.LogInformation("Database migrations applied successfully on startup.");

			var missingTables = new List<string>();
			foreach (var table in RequiredTables)
			{
				var exists = db.Database
					.SqlQueryRaw<int>(
						"SELECT COUNT(1) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = {0}",
						table).AsEnumerable().FirstOrDefault() > 0;

				if (!exists) missingTables.Add(table);
			}

			if (missingTables.Count == 0)
			{
				logger.LogInformation("Database schema verification passed for critical tables.");
				return;
			}

			logger.LogError("Database schema verification failed. Missing critical tables: {MissingTables}",
				string.Join(", ", missingTables));

			var failStartup = app.Configuration.GetValue<bool?>("Database:FailStartupOnMissingTables") ?? true;
			if (app.Environment.IsProduction() && failStartup)
				throw new InvalidOperationException(
					$"Missing critical database tables: {string.Join(", ", missingTables)}");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to apply database migrations on startup.");
			throw;
		}
	}
}
