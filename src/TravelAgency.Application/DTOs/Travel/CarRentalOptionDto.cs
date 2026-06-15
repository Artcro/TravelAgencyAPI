namespace TravelAgency.Application.DTOs.Travel;

public sealed class CarRentalOptionDto
{
	public string Provider { get; set; } = "Mock";
	public string ProviderCarId { get; set; } = "";
	public string Model { get; set; } = "";
	public int Seats { get; set; }
	public string Transmission { get; set; } = "";
	public string Mileage { get; set; } = "";
	public MoneyDto? PricePerDay { get; set; }
	public string? ImageUrl { get; set; }
}
