using TravelAgency.Domain.ValueObjects;

namespace TravelAgency.Application.DTOs.Travel;

public sealed class TripSearchRequest
{
	public string Origin { get; set; } = "";
	public string Destination { get; set; } = "";
	public DateOnly DepartureDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);
	public DateOnly? ReturnDate { get; set; }
	public int Adults { get; set; } = 1;
	public int Children { get; set; }
	public int Infants { get; set; }
	public string Currency { get; set; } = Domain.ValueObjects.Currency.Default;
	public string TravelClass { get; set; } = TravelClassParser.DefaultWireValue;
	public int MaxFlightResults { get; set; } = 10;
	public bool IncludeHotels { get; set; } = true;
	public bool IncludeActivities { get; set; } = true;
}
