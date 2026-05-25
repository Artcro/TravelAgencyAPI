namespace TravelAgency.Application.DTOs.Travel;

public sealed record SavedTripDetailDto(
	Guid Id,
	string Name,
	string Status,
	DateTime CreatedAtUtc,
	Guid SearchId,
	Guid? UserId,
	string? SelectedFlightProviderOfferId,
	string? SelectedHotelProviderHotelId);
