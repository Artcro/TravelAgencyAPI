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
using TravelAgency.Infrastructure.Providers.Amadeus;
using TravelAgency.Infrastructure.Providers.Duffel;
using TravelAgency.Infrastructure.Providers.Mock;

namespace TravelAgency.Infrastructure.Config;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AmadeusOptions>(configuration.GetSection(AmadeusOptions.SectionName));
        services.Configure<TravelProvidersOptions>(configuration.GetSection(TravelProvidersOptions.SectionName));
        services.Configure<DuffelOptions>(configuration.GetSection(DuffelOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
        services.Configure<TravelSearchDefaultsOptions>(configuration.GetSection(TravelSearchDefaultsOptions.SectionName));
        services.AddMemoryCache();
        services.AddDbContext<TravelDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>().AddEntityFrameworkStores<TravelDbContext>().AddDefaultTokenProviders();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITripSearchService, TripSearchService>();
        services.AddScoped<ISavedTripService, SavedTripService>();
        services.AddScoped<IFrontendTravelTicketService, FrontendTravelTicketService>();
        services.AddScoped<ILocationProvider>(sp =>
        {
            var providers = sp.GetRequiredService<IOptions<TravelProvidersOptions>>().Value;
            var amadeus = sp.GetRequiredService<IOptions<AmadeusOptions>>().Value;
            var location = providers.LocationProvider;
            if (string.Equals(location, "Amadeus", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(amadeus.ClientId) && !string.IsNullOrWhiteSpace(amadeus.ClientSecret))
                return sp.GetRequiredService<AmadeusLocationProvider>();
            return sp.GetRequiredService<MockLocationProvider>();
        });
        services.AddScoped<IFlightProvider>(sp =>
        {
            var provider = sp.GetRequiredService<IOptions<TravelProvidersOptions>>().Value.FlightProvider;
            return string.Equals(provider, "Amadeus", StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<AmadeusFlightProvider>()
                : sp.GetRequiredService<DuffelFlightProvider>();
        });
        services.AddScoped<IHotelProvider, MockHotelProvider>();
        services.AddScoped<IActivityProvider, MockActivityProvider>();
        services.AddScoped<AmadeusAuthClient>();
        services.AddScoped<AmadeusLocationProvider>();
        services.AddScoped<AmadeusFlightProvider>();
        services.AddScoped<DuffelFlightProvider>();
        services.AddScoped<MockLocationProvider>();
        services.AddHttpClient("amadeus", c => c.Timeout = TimeSpan.FromSeconds(20));
        services.AddHttpClient("duffel", c => c.Timeout = TimeSpan.FromSeconds(20));
        services.AddHttpClient();
        return services;
    }
}
