using System.Text.Json.Serialization;

namespace TravelAgency.Infrastructure.Providers.Duffel;

public sealed class DuffelOfferRequestEnvelope { [JsonPropertyName("data")] public DuffelOfferRequestData Data { get; set; } = new(); }
public sealed class DuffelOfferRequestData
{
    [JsonPropertyName("slices")] public List<DuffelSliceRequest> Slices { get; set; } = [];
    [JsonPropertyName("passengers")] public List<DuffelPassengerRequest> Passengers { get; set; } = [];
    [JsonPropertyName("cabin_class")] public string? CabinClass { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
}
public sealed class DuffelSliceRequest { [JsonPropertyName("origin")] public string Origin { get; set; } = string.Empty; [JsonPropertyName("destination")] public string Destination { get; set; } = string.Empty; [JsonPropertyName("departure_date")] public string DepartureDate { get; set; } = string.Empty; }
public sealed class DuffelPassengerRequest { [JsonPropertyName("type")] public string Type { get; set; } = "adult"; }
