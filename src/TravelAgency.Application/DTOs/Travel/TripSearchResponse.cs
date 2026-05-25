namespace TravelAgency.Application.DTOs.Travel;

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
