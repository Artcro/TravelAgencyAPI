using TravelAgency.Domain.ValueObjects;

namespace TravelAgency.Application.DTOs.Travel;

public sealed class FlightOptionDto
{
	public string Provider { get; set; } = "Duffel";
	public string ProviderOfferId { get; set; } = "";
	public string AirlineCode { get; set; } = "";
	public string? AirlineName { get; set; }
	public MoneyDto TotalPrice { get; set; } = new(0, Currency.Default);
	public string Duration { get; set; } = "";
	public int Stops { get; set; }
	public List<TripSegmentDto> OutboundSegments { get; set; } = [];
	public List<TripSegmentDto> ReturnSegments { get; set; } = [];
	public string? DeepLink { get; set; }
}
