namespace TravelAgency.Application.DTOs.Travel;

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
