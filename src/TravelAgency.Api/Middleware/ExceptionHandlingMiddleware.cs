using Microsoft.AspNetCore.Mvc;
namespace TravelAgency.Api.Middleware;
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger){ public async Task Invoke(HttpContext context){ try{ await next(context);}catch(Exception ex){ logger.LogError(ex,"Unhandled exception"); context.Response.StatusCode=500; await context.Response.WriteAsJsonAsync(new ProblemDetails{Status=500,Title="Unexpected error",Detail="An unexpected error occurred.",Instance=context.Request.Path}); } } }
