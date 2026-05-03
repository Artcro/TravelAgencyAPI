namespace TravelAgency.Application.Auth;

public interface IRefreshTokenService
{
	string GenerateRefreshToken();
	string HashRefreshToken(string token);
}