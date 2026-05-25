using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TravelAgency.Application.Config;

namespace TravelAgency.Api.Config;

internal static class AuthExtensions
{
	public static IServiceCollection AddTravelJwtAuth(this IServiceCollection services, IConfiguration configuration)
	{
		var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(opts =>
			{
				opts.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateIssuerSigningKey = true,
					ValidateLifetime = true,
					ValidIssuer = jwt.Issuer,
					ValidAudience = jwt.Audience,
					IssuerSigningKey = key
				};
			});

		services.AddAuthorization();
		return services;
	}
}
