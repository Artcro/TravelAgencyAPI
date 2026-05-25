using TravelAgency.Api.Options;

namespace TravelAgency.Api.Config;

internal static class CorsExtensions
{
	public const string PolicyName = "DefaultCors";

	public static IServiceCollection AddTravelCors(this IServiceCollection services, IConfiguration configuration)
	{
		var cors = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
		services.AddCors(options =>
		{
			options.AddPolicy(PolicyName, policy =>
			{
				if (cors.AllowAnyOrigin)
					policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
				else
					policy.WithOrigins(cors.AllowedOrigins).AllowAnyHeader().AllowAnyMethod();
			});
		});

		return services;
	}
}
