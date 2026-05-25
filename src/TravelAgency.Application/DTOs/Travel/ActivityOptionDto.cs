namespace TravelAgency.Application.DTOs.Travel;

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
