using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using TravelAgency.Api.Middleware;
using TravelAgency.Api.Options;
using TravelAgency.Api.Services;
using TravelAgency.Application.Config;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Config;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<TravelAgency.Infrastructure.SecurityOptions>(builder.Configuration.GetSection(TravelAgency.Infrastructure.SecurityOptions.SectionName));
builder.Services.Configure<AmadeusOptions>(builder.Configuration.GetSection(AmadeusOptions.SectionName));
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
    options.AddFixedWindowLimiter("auth-strict", o => { o.PermitLimit = 10; o.Window = TimeSpan.FromMinutes(1); o.QueueLimit = 0; });
    options.AddFixedWindowLimiter("search-medium", o => { o.PermitLimit = 30; o.Window = TimeSpan.FromMinutes(1); o.QueueLimit = 5; o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; });
    options.AddFixedWindowLimiter("locations-relaxed", o => { o.PermitLimit = 60; o.Window = TimeSpan.FromMinutes(1); o.QueueLimit = 10; o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; });
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
builder.Services.AddCors(o => o.AddPolicy("DefaultCors", p => p.WithOrigins(cors.AllowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors("DefaultCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
