namespace TravelAgency.Application.DTOs.Travel;

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
