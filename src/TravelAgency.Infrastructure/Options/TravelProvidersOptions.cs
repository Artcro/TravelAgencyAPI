namespace TravelAgency.Infrastructure.Options;

public sealed class TravelProvidersOptions
{
    public const string SectionName = "TravelProviders";
    public string FlightProvider { get; set; } = "Duffel";
    public string LocationProvider { get; set; } = "Mock";
}
