using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TravelAgency.Api.Middleware;
using TravelAgency.Api.Options;
using TravelAgency.Api.Services;
using TravelAgency.Application.Config;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Config;
using TravelAgency.Infrastructure.Database;


var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port)) builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.AddFixedWindowLimiter("auth-strict", o =>
	{
		o.PermitLimit = 10;
		o.Window = TimeSpan.FromMinutes(1);
		o.QueueLimit = 0;
	});

	options.AddFixedWindowLimiter("search-medium", o =>
	{
		o.PermitLimit = 30;
		o.Window = TimeSpan.FromMinutes(1);
		o.QueueLimit = 5;
		o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
	});

	options.AddFixedWindowLimiter("locations-relaxed", o =>
	{
		o.PermitLimit = 60;
		o.Window = TimeSpan.FromMinutes(1);
		o.QueueLimit = 10;
		o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
	});
});

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization();

var cors = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
builder.Services.AddCors(options =>
{
	options.AddPolicy("DefaultCors", policy =>
	{
		if (cors.AllowAnyOrigin)
			policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
		else
			policy.WithOrigins(cors.AllowedOrigins).AllowAnyHeader().AllowAnyMethod();
	});
});

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
var swaggerEnabled = app.Configuration.GetValue<bool?>("Swagger:Enabled") ?? app.Environment.IsDevelopment();
if (swaggerEnabled)
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

var applyMigrationsOnStartup = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
if (applyMigrationsOnStartup)
{
	using var scope = app.Services.CreateScope();
	var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigrations");
	try
	{
		var db = scope.ServiceProvider.GetRequiredService<TravelDbContext>();
		db.Database.Migrate();
		logger.LogInformation("Database migrations applied successfully on startup.");

		var requiredTables = new[]
		{
			"ProviderRequestLogs", "TripSearches", "SavedTrips", "AuditLogs", "Airports", "AirportDataSyncStatuses"
		};
		var missingTables = new List<string>();
		foreach (var table in requiredTables)
		{
			var exists = db.Database
				.SqlQueryRaw<int>(
					"SELECT COUNT(1) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = {0}",
					table).AsEnumerable().FirstOrDefault() > 0;

			if (!exists) missingTables.Add(table);
		}

		if (missingTables.Count == 0)
		{
			logger.LogInformation("Database schema verification passed for critical tables.");
		}
		else
		{
			logger.LogError("Database schema verification failed. Missing critical tables: {MissingTables}",
				string.Join(", ", missingTables));

			var failStartup = app.Configuration.GetValue<bool?>("Database:FailStartupOnMissingTables") ?? true;
			if (app.Environment.IsProduction() && failStartup)
				throw new InvalidOperationException(
					$"Missing critical database tables: {string.Join(", ", missingTables)}");
		}
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Failed to apply database migrations on startup.");
		throw;
	}
}

app.UseCors("DefaultCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
