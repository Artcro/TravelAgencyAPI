namespace TravelAgency.Api.Options;
public sealed class CorsOptions { public const string SectionName="Cors"; public bool AllowAnyOrigin { get; set; } = true; public string[] AllowedOrigins { get; set; } = []; }
