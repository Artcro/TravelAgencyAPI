using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TravelAgency.Api.Middleware;

namespace TravelAgency.Tests.Travel;

public sealed class ExceptionHandlingMiddlewareTests
{
	[Fact]
	public async Task InvalidOperationException_Maps_To_502()
	{
		var ctx = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
		var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("boom"),
			NullLogger<ExceptionHandlingMiddleware>.Instance);

		await middleware.Invoke(ctx);

		Assert.Equal(StatusCodes.Status502BadGateway, ctx.Response.StatusCode);
	}

	[Fact]
	public async Task Generic_ArgumentException_Now_Falls_Through_To_500()
	{
		// Phase 5: validators no longer throw, so an ArgumentException from any
		// random library code must not be misreported as a 400.
		var ctx = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
		var middleware = new ExceptionHandlingMiddleware(_ => throw new ArgumentException("bad arg"),
			NullLogger<ExceptionHandlingMiddleware>.Instance);

		await middleware.Invoke(ctx);

		Assert.Equal(StatusCodes.Status500InternalServerError, ctx.Response.StatusCode);
	}

	[Fact]
	public async Task Unhandled_Exception_Maps_To_500()
	{
		var ctx = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
		var middleware = new ExceptionHandlingMiddleware(_ => throw new Exception("???"),
			NullLogger<ExceptionHandlingMiddleware>.Instance);

		await middleware.Invoke(ctx);

		Assert.Equal(StatusCodes.Status500InternalServerError, ctx.Response.StatusCode);
		ctx.Response.Body.Seek(0, SeekOrigin.Begin);
		using var doc = JsonDocument.Parse(ctx.Response.Body);
		Assert.Equal("Unexpected error", doc.RootElement.GetProperty("title").GetString());
	}
}
