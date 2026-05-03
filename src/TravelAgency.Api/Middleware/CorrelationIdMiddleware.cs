namespace TravelAgency.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
	public const string HeaderName = "X-Correlation-Id";

	public async Task Invoke(HttpContext context)
	{
		var cid = context.Request.Headers[HeaderName].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
		context.TraceIdentifier = cid;
		context.Response.Headers[HeaderName] = cid;
		await next(context);
	}
}