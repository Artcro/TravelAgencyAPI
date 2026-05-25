namespace TravelAgency.Application.DTOs.Travel;

public sealed record SavedTripSummaryDto(Guid Id, string Name, string Status, DateTime CreatedAtUtc);
