namespace TravelAgency.Application.DTOs.Travel;

public sealed class SaveTripRequest
{
	public Guid SearchId { get; set; }
	public string? SelectedFlightProviderOfferId { get; set; }
	public string? SelectedHotelProviderHotelId { get; set; }
	public List<string> SelectedActivityIds { get; set; } = [];
	public string Name { get; set; } = "";
}
