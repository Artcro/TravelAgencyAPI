using TravelAgency.Api.Config;
using TravelAgency.Api.Middleware;
using TravelAgency.Api.Options;
using TravelAgency.Api.Services;
using TravelAgency.Application.Config;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Config;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port)) builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));

builder.Services
	.AddApplication()
	.AddInfrastructure(builder.Configuration)
	.AddHttpContextAccessor()
	.AddScoped<ICurrentUserService, CurrentUserService>()
	.AddTravelJwtAuth(builder.Configuration)
	.AddTravelCors(builder.Configuration)
	.AddTravelRateLimiting()
	.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
var swaggerEnabled = app.Configuration.GetValue<bool?>("Swagger:Enabled") ?? app.Environment.IsDevelopment();
if (swaggerEnabled)
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

await app.ApplyMigrationsAndVerifyAsync();
app.UseCors(CorsExtensions.PolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
