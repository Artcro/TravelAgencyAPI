namespace TravelAgency.Application.DTOs.Travel;

public sealed class TravelTicketSearchRequest
{
	public string Origin { get; set; } = "";
	public string Destination { get; set; } = "";
	public DateOnly DepartureDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);
	public DateOnly? ReturnDate { get; set; }
	public int Adults { get; set; } = 1;
	public int Children { get; set; }
	public int Infants { get; set; }
	public string Currency { get; set; } = "BRL";
	public string TravelClass { get; set; } = "ECONOMY";
	public int MaxResults { get; set; } = 10;
}
