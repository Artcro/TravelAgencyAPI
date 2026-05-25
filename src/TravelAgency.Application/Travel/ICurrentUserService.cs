namespace TravelAgency.Application.Travel;

public interface ICurrentUserService
{
	Guid? UserId { get; }
	string? Email { get; }
	bool IsAuthenticated { get; }
}
