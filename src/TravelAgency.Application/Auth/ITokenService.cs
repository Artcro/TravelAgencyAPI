namespace TravelAgency.Application.Auth;

public interface ITokenService
{
	string CreateAccessToken(Guid userId, string email, string displayName);
}