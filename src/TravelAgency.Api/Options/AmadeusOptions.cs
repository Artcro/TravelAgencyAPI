namespace TravelAgency.Api.Options;
public sealed class AmadeusOptions { public const string SectionName="Amadeus"; public string BaseUrl { get; set; } = "https://test.api.amadeus.com"; public string ClientId { get; set; } = string.Empty; public string ClientSecret { get; set; } = string.Empty; }
