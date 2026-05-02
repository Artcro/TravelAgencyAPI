using Microsoft.Extensions.DependencyInjection;
using TravelAgency.Application.Auth;
using TravelAgency.Application.Travel;
namespace TravelAgency.Application.Config;
public static class DependencyInjection { public static IServiceCollection AddApplication(this IServiceCollection services){ services.AddScoped<ITokenService, TokenService>(); services.AddScoped<IRefreshTokenService, RefreshTokenService>(); services.AddSingleton<TripSearchRequestValidator>(); services.AddSingleton<TripResultNormalizer>(); services.AddSingleton<TravelTicketSearchRequestValidator>(); return services; } }
