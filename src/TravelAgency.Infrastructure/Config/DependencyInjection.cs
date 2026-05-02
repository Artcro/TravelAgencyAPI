using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelAgency.Application.Auth;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;
using TravelAgency.Infrastructure.Options;
using TravelAgency.Infrastructure.Providers.Amadeus;
using TravelAgency.Infrastructure.Providers.Mock;

namespace TravelAgency.Infrastructure.Config;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AmadeusOptions>(configuration.GetSection(AmadeusOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
        services.AddMemoryCache();
        services.AddDbContext<TravelDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>().AddEntityFrameworkStores<TravelDbContext>().AddDefaultTokenProviders();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITripSearchService, TripSearchService>();
        services.AddScoped<ISavedTripService, SavedTripService>();
        services.AddScoped<ILocationProvider, AmadeusLocationProvider>();
        services.AddScoped<IFlightProvider, AmadeusFlightProvider>();
        services.AddScoped<IHotelProvider, MockHotelProvider>();
        services.AddScoped<IActivityProvider, MockActivityProvider>();
        services.AddScoped<AmadeusAuthClient>();
        services.AddHttpClient();
        return services;
    }
}
