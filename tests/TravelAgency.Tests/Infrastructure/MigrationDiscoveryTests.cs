using Microsoft.EntityFrameworkCore;
using TravelAgency.Infrastructure.Database;

namespace TravelAgency.Tests.Infrastructure;

public class MigrationDiscoveryTests
{
	[Fact]
	public void TravelDbContext_MigrationAssembly_Has_Discoverable_Migrations()
	{
		var options = new DbContextOptionsBuilder<TravelDbContext>()
			.UseNpgsql("Host=localhost;Database=dummy;Username=dummy;Password=dummy")
			.Options;

		using var db = new TravelDbContext(options);
		var migrations = db.Database.GetMigrations().ToList();

		Assert.NotEmpty(migrations);
		Assert.Contains(migrations, migration => migration.Contains("InitialCreate", StringComparison.Ordinal));
	}
}