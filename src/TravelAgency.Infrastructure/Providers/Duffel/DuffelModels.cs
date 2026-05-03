using System.Text.Json.Serialization;

namespace TravelAgency.Infrastructure.Providers.Duffel;

public sealed class DuffelOfferRequestEnvelope
{
	[JsonPropertyName("data")]
	public DuffelOfferRequestData Data { get; set; } = new();
}

public sealed class DuffelOfferRequestData
{
	[JsonPropertyName("slices")]
	public List<DuffelSliceRequest> Slices { get; set; } = [];
	[JsonPropertyName("passengers")]
	public List<DuffelPassengerRequest> Passengers { get; set; } = [];
	[JsonPropertyName("cabin_class")]
	public string? CabinClass { get; set; }
	[JsonPropertyName("currency")]
	public string? Currency { get; set; }
	[JsonPropertyName("supplier_timeout")]
	public int SupplierTimeout { get; set; }
	[JsonPropertyName("max_connections")]
	public int MaxConnections { get; set; }
	[JsonPropertyName("return_offers")]
	public bool ReturnOffers { get; set; }
}

public sealed class DuffelSliceRequest
{
	[JsonPropertyName("origin")]
	public string Origin { get; set; } = string.Empty;
	[JsonPropertyName("destination")]
	public string Destination { get; set; } = string.Empty;
	[JsonPropertyName("departure_date")]
	public string DepartureDate { get; set; } = string.Empty;
}

public sealed class DuffelPassengerRequest
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = "adult";
}

public sealed class DuffelOfferResponseEnvelope
{
	[JsonPropertyName("data")]
	public DuffelOfferResponseData? Data { get; set; }
}

public sealed class DuffelOfferResponseData
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }
	[JsonPropertyName("offers")]
	public List<DuffelOffer>? Offers { get; set; }
}

public sealed class DuffelListOffersEnvelope
{
	[JsonPropertyName("data")]
	public List<DuffelOffer>? Data { get; set; }
}

public sealed class DuffelOffer
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }
	[JsonPropertyName("total_amount")]
	public string? TotalAmount { get; set; }
	[JsonPropertyName("total_currency")]
	public string? TotalCurrency { get; set; }
	[JsonPropertyName("owner")]
	public DuffelOwner? Owner { get; set; }
	[JsonPropertyName("slices")]
	public List<DuffelSlice>? Slices { get; set; }
}

public sealed class DuffelOwner
{
	[JsonPropertyName("iata_code")]
	public string? IataCode { get; set; }
	[JsonPropertyName("name")]
	public string? Name { get; set; }
}

public sealed class DuffelSlice
{
	[JsonPropertyName("duration")]
	public string? Duration { get; set; }
	[JsonPropertyName("segments")]
	public List<DuffelSegment>? Segments { get; set; }
}

public sealed class DuffelSegment
{
	[JsonPropertyName("departing_at")]
	public string? DepartingAt { get; set; }
	[JsonPropertyName("arriving_at")]
	public string? ArrivingAt { get; set; }
	[JsonPropertyName("origin")]
	public DuffelIataRef? Origin { get; set; }
	[JsonPropertyName("destination")]
	public DuffelIataRef? Destination { get; set; }
	[JsonPropertyName("duration")]
	public string? Duration { get; set; }
	[JsonPropertyName("marketing_carrier")]
	public DuffelIataRef? MarketingCarrier { get; set; }
	[JsonPropertyName("marketing_carrier_flight_number")]
	public string? MarketingCarrierFlightNumber { get; set; }
}

public sealed class DuffelIataRef
{
	[JsonPropertyName("iata_code")]
	public string? IataCode { get; set; }
}