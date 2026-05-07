using System.Text.Json.Serialization;

namespace TravelAgency.Application.DTOs.Travel;

public record MoneyDto(decimal Amount, string Currency);

public record LocationSuggestionDto(
	string Code,
	string Name,
	string Type,
	string CountryCode,
	string DisplayName,
	string? City = null,
	string? CountryName = null);

public sealed class AirportAutocompleteDto
{
	[JsonPropertyName("iata")]
	public string Iata { get; set; } = "";
	[JsonPropertyName("cidade")]
	public string Cidade { get; set; } = "";
	[JsonPropertyName("pais")]
	public string Pais { get; set; } = "";
	[JsonPropertyName("nome")]
	public string Nome { get; set; } = "";
}

public record LocationSummaryDto(string Code, string Name);

public sealed class TripSegmentDto
{
	public string Origin { get; set; } = "";
	public string Destination { get; set; } = "";
	public DateTime DepartureAt { get; set; }
	public DateTime ArrivalAt { get; set; }
	public string CarrierCode { get; set; } = "";
	public string FlightNumber { get; set; } = "";
	public string Duration { get; set; } = "";
}

public sealed class FlightOptionDto
{
	public string Provider { get; set; } = "Amadeus";
	public string ProviderOfferId { get; set; } = "";
	public string AirlineCode { get; set; } = "";
	public string? AirlineName { get; set; }
	public MoneyDto TotalPrice { get; set; } = new(0, "BRL");
	public string Duration { get; set; } = "";
	public int Stops { get; set; }
	public List<TripSegmentDto> OutboundSegments { get; set; } = [];
	public List<TripSegmentDto> ReturnSegments { get; set; } = [];
	public string? DeepLink { get; set; }
}

public sealed class HotelOptionDto
{
	public string Provider { get; set; } = "Mock";
	public string ProviderHotelId { get; set; } = "";
	public string Name { get; set; } = "";
	public string CityCode { get; set; } = "";
	public int? Rating { get; set; }
	public MoneyDto? PricePerNight { get; set; }
	public string? ImageUrl { get; set; }
	public string? Address { get; set; }
}

public sealed class ActivityOptionDto
{
	public string Provider { get; set; } = "Mock";
	public string ProviderActivityId { get; set; } = "";
	public string Title { get; set; } = "";
	public string Description { get; set; } = "";
	public MoneyDto? Price { get; set; }
	public string? Duration { get; set; }
	public string? ImageUrl { get; set; }
}

public sealed class TripSearchResponse
{
	public Guid SearchId { get; set; }
	public LocationSummaryDto Origin { get; set; } = new("", "");
	public LocationSummaryDto Destination { get; set; } = new("", "");
	public DateOnly DepartureDate { get; set; }
	public DateOnly? ReturnDate { get; set; }
	public string Currency { get; set; } = "BRL";
	public List<FlightOptionDto> Flights { get; set; } = [];
	public List<HotelOptionDto> Hotels { get; set; } = [];
	public List<ActivityOptionDto> Activities { get; set; } = [];
	public List<string> Warnings { get; set; } = [];
}

public sealed class SaveTripRequest
{
	public Guid SearchId { get; set; }
	public string? SelectedFlightProviderOfferId { get; set; }
	public string? SelectedHotelProviderHotelId { get; set; }
	public List<string> SelectedActivityIds { get; set; } = [];
	public string Name { get; set; } = "";
}

public record SavedTripResponse(Guid TripId, string Status);
