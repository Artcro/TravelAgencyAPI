namespace TravelAgency.Application.Config;
public sealed class JwtOptions { public const string SectionName="Jwt"; public string Issuer { get; set; } = "TravelAgencyApi"; public string Audience { get; set; } = "TravelAgencyFrontend"; public string Secret { get; set; } = string.Empty; public int AccessTokenMinutes { get; set; } = 15; public int RefreshTokenDays { get; set; } = 14; }
