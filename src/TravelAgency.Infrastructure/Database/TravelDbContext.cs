using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure.Database;

public sealed class TravelDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
	public TravelDbContext(DbContextOptions<TravelDbContext> options) : base(options)
	{
	}

	public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
	public DbSet<TripSearchEntity> TripSearches => Set<TripSearchEntity>();
	public DbSet<SavedTripEntity> SavedTrips => Set<SavedTripEntity>();
	public DbSet<ProviderRequestLogEntity> ProviderRequestLogs => Set<ProviderRequestLogEntity>();
	public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();
	public DbSet<AirportEntity> Airports => Set<AirportEntity>();
	public DbSet<AirportDataSyncStatusEntity> AirportDataSyncStatuses => Set<AirportDataSyncStatusEntity>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		builder.Entity<TripSearchEntity>().Property(x => x.RequestJson).HasColumnType("jsonb");
		builder.Entity<TripSearchEntity>().Property(x => x.ResponseJson).HasColumnType("jsonb");
		builder.Entity<SavedTripEntity>().Property(x => x.SelectedActivityIdsJson).HasColumnType("jsonb");
		builder.Entity<AuditLogEntity>().Property(x => x.MetadataJson).HasColumnType("jsonb");
		builder.Entity<SavedTripEntity>().HasIndex(x => x.UserId);
		builder.Entity<SavedTripEntity>().HasIndex(x => x.SearchId);
		builder.Entity<SavedTripEntity>().HasIndex(x => x.CreatedAtUtc);
		builder.Entity<TripSearchEntity>().HasIndex(x => x.CreatedAtUtc);
		builder.Entity<ProviderRequestLogEntity>().HasIndex(x => x.CreatedAtUtc);
		builder.Entity<AuditLogEntity>().HasIndex(x => x.CreatedAtUtc);

		builder.Entity<AirportEntity>(entity =>
		{
			entity.HasKey(x => x.IataCode);
			entity.Property(x => x.IataCode).HasMaxLength(3);
			entity.Property(x => x.IcaoCode).HasMaxLength(4);
			entity.Property(x => x.Ident).HasMaxLength(16);
			entity.Property(x => x.CountryCode).HasMaxLength(2);
			entity.Property(x => x.AirportType).HasMaxLength(32);
			entity.HasIndex(x => x.City);
			entity.HasIndex(x => x.CitySearch);
			entity.HasIndex(x => x.NameSearch);
			entity.HasIndex(x => x.CountrySearch);
			entity.HasIndex(x => x.CountryCode);
			entity.HasIndex(x => x.Name);
			entity.HasIndex(x => x.IsActive);
			entity.HasIndex(x => x.LastSyncedAtUtc);
		});

		builder.Entity<AirportDataSyncStatusEntity>(entity =>
		{
			entity.HasKey(x => x.Source);
			entity.Property(x => x.Source).HasMaxLength(64);
		});
	}
}
