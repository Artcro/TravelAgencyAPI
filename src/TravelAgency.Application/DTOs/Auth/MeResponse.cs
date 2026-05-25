namespace TravelAgency.Application.DTOs.Auth;

public sealed record MeResponse(Guid? UserId, string? Email, string? DisplayName);
