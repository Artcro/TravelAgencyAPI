using Microsoft.Extensions.DependencyInjection;
using TravelAgency.Application.Auth;
namespace TravelAgency.Application.Config;
public static class DependencyInjection { public static IServiceCollection AddApplication(this IServiceCollection services){ services.AddScoped<ITokenService, TokenService>(); services.AddScoped<IRefreshTokenService, RefreshTokenService>(); return services; } }
