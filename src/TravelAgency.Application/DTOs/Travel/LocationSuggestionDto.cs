namespace TravelAgency.Application.DTOs.Travel;

public record LocationSuggestionDto(
	string Code,
	string Name,
	string Type,
	string CountryCode,
	string DisplayName,
	string? City = null,
	string? CountryName = null);
