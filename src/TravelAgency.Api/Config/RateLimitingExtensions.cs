using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace TravelAgency.Api.Config;

internal static class RateLimitingExtensions
{
	// Permits/window/queue tuned for the three traffic shapes the API gets:
	// auth is bursty + low-volume, search is moderate, locations is high-volume.
	private const int AuthStrictPermitLimit = 10;
	private const int AuthStrictQueueLimit = 0;
	private const int SearchMediumPermitLimit = 30;
	private const int SearchMediumQueueLimit = 5;
	private const int LocationsRelaxedPermitLimit = 60;
	private const int LocationsRelaxedQueueLimit = 10;
	private static readonly TimeSpan FixedWindow = TimeSpan.FromMinutes(1);

	public static IServiceCollection AddTravelRateLimiting(this IServiceCollection services)
	{
		services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
			options.AddFixedWindowLimiter("auth-strict", o =>
			{
				o.PermitLimit = AuthStrictPermitLimit;
				o.Window = FixedWindow;
				o.QueueLimit = AuthStrictQueueLimit;
			});

			options.AddFixedWindowLimiter("search-medium", o =>
			{
				o.PermitLimit = SearchMediumPermitLimit;
				o.Window = FixedWindow;
				o.QueueLimit = SearchMediumQueueLimit;
				o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			});

			options.AddFixedWindowLimiter("locations-relaxed", o =>
			{
				o.PermitLimit = LocationsRelaxedPermitLimit;
				o.Window = FixedWindow;
				o.QueueLimit = LocationsRelaxedQueueLimit;
				o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			});
		});

		return services;
	}
}
