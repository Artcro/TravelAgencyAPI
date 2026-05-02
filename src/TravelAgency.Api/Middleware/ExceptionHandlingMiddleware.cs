using Microsoft.AspNetCore.Mvc;

namespace TravelAgency.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 400,
                Title = "Invalid request",
                Detail = ex.Message,
                Instance = context.Request.Path
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Operation failed");
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 502,
                Title = "Provider failure",
                Detail = ex.Message,
                Instance = context.Request.Path
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = 500, Title = "Unexpected error", Detail = "An unexpected error occurred.", Instance = context.Request.Path });
        }
    }
}
