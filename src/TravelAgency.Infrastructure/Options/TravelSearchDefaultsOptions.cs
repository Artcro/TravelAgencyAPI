namespace TravelAgency.Infrastructure.Options;

public sealed class TravelSearchDefaultsOptions
{
    public const string SectionName = "TravelSearchDefaults";
    public string? DefaultOrigin { get; set; }
}
