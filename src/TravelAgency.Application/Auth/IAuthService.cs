using System.Security.Claims;
using TravelAgency.Application.DTOs.Auth;

namespace TravelAgency.Application.Auth;

public interface IAuthService
{
	Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken);
	Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken);

	Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, string? ipAddress,
		CancellationToken cancellationToken);

	Task LogoutAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken);
	Task<ClaimsPrincipal?> GetMeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}