namespace TravelAgency.Infrastructure.Providers.Duffel;
public sealed class DuffelOptions
{
    public const string SectionName = "Duffel";
    public string BaseUrl { get; set; } = "https://api.duffel.com";
    public string AccessToken { get; set; } = string.Empty;
    public string Version { get; set; } = "v2";
    public long MaxResponseBytes { get; set; } = 5 * 1024 * 1024;
    public int TimeoutSeconds { get; set; } = 45;
    public int SupplierTimeoutMilliseconds { get; set; } = 15_000;
    public int MaxConnections { get; set; } = 1;
    public int MaxOffersToRead { get; set; } = 10;
    public bool UseReturnOffers { get; set; } = true;
    public bool ReturnEmptyOnTimeout { get; set; } = true;
}
