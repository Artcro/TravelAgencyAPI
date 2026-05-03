namespace TravelAgency.Infrastructure.Providers.Duffel;

public sealed class DuffelOptions
{
    public const string SectionName = "Duffel";
    public string BaseUrl { get; set; } = "https://api.duffel.com";
    public string AccessToken { get; set; } = string.Empty;
    public string Version { get; set; } = "v2";
}
