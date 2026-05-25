using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TravelAgency.Application.Auth;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;
using TravelAgency.Infrastructure.Options;
using TravelAgency.Infrastructure.Providers.Duffel;
using TravelAgency.Infrastructure.Providers.Local;
using TravelAgency.Infrastructure.Providers.Mock;
using TravelAgency.Infrastructure.Services.Airports;

namespace TravelAgency.Infrastructure.Config;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<TravelProvidersOptions>(configuration.GetSection(TravelProvidersOptions.SectionName));
		services.Configure<DuffelOptions>(configuration.GetSection(DuffelOptions.SectionName));
		services.Configure<AirportDataSyncOptions>(configuration.GetSection(AirportDataSyncOptions.SectionName));
		services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
		services.Configure<TravelSearchDefaultsOptions>(
			configuration.GetSection(TravelSearchDefaultsOptions.SectionName));

		services.AddMemoryCache();
		services.AddDbContext<TravelDbContext>(opt =>
			opt.UseNpgsql(
				configuration.GetConnectionString("DefaultConnection"),
				npgsql => npgsql.MigrationsAssembly(typeof(TravelDbContext).Assembly.FullName)));

		services.AddIdentity<ApplicationUser, IdentityRole<Guid>>().AddEntityFrameworkStores<TravelDbContext>()
			.AddDefaultTokenProviders();

		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<ITripSearchService, TripSearchService>();
		services.AddScoped<ISavedTripService, SavedTripService>();
		services.AddScoped<IFrontendTravelTicketService, FrontendTravelTicketService>();
		services.AddScoped<ILocationProvider>(sp =>
		{
			var providers = sp.GetRequiredService<IOptions<TravelProvidersOptions>>().Value;
			return IsLocalLocationProvider(providers.LocationProvider)
				? sp.GetRequiredService<LocalAirportLocationProvider>()
				: sp.GetRequiredService<MockLocationProvider>();
		});

		services.AddScoped<IFlightProvider, DuffelFlightProvider>();

		services.AddScoped<IHotelProvider, MockHotelProvider>();
		services.AddScoped<IActivityProvider, MockActivityProvider>();
		services.AddScoped<DuffelFlightProvider>();
		services.AddScoped<LocalAirportLocationProvider>();
		services.AddScoped<MockLocationProvider>();
		services.AddScoped<IAirportDataSyncService, OurAirportsDataSyncService>();
		services.AddHostedService<OurAirportsSyncHostedService>();
		services.AddHttpClient("duffel", (sp, c) =>
		{
			var opts = sp.GetRequiredService<IOptions<DuffelOptions>>().Value;
			var timeoutSeconds = opts.TimeoutSeconds > 0 ? opts.TimeoutSeconds : 45;
			c.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
		});
		services.AddHttpClient("ourairports", (sp, c) =>
		{
			var opts = sp.GetRequiredService<IOptions<AirportDataSyncOptions>>().Value;
			c.Timeout = TimeSpan.FromSeconds(Math.Max(15, opts.RequestTimeoutSeconds));
			c.DefaultRequestHeaders.UserAgent.ParseAdd("TravelAgencyAPI/1.0");
		});

		services.AddHttpClient();
		return services;
	}

	private static bool IsLocalLocationProvider(string provider)
	{
		return string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase) ||
		       string.Equals(provider, "Database", StringComparison.OrdinalIgnoreCase) ||
		       string.Equals(provider, "OurAirports", StringComparison.OrdinalIgnoreCase);
	}
}
