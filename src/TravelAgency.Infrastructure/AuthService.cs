using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TravelAgency.Application.Auth;
using TravelAgency.Application.Config;
using TravelAgency.Application.DTOs.Auth;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure;

public sealed class AuthService(
	UserManager<ApplicationUser> userManager,
	SignInManager<ApplicationUser> signInManager,
	TravelDbContext dbContext,
	ITokenService tokenService,
	IRefreshTokenService refreshTokenService,
	IOptions<JwtOptions> jwtOptions) : IAuthService
{
	public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress,
		CancellationToken cancellationToken)
	{
		var user = new ApplicationUser
		{
			Id = Guid.NewGuid(), Email = request.Email, UserName = request.Email, DisplayName = request.DisplayName,
			CreatedAtUtc = DateTime.UtcNow
		};

		var result = await userManager.CreateAsync(user, request.Password);
		if (!result.Succeeded)
			throw new InvalidOperationException(string.Join(";", result.Errors.Select(e => e.Description)));

		return await BuildAuthResponseAsync(user, ipAddress, cancellationToken);
	}

	public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress,
		CancellationToken cancellationToken)
	{
		var user = await userManager.FindByEmailAsync(request.Email) ??
		           throw new UnauthorizedAccessException("Invalid credentials.");

		var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
		if (!result.Succeeded) throw new UnauthorizedAccessException("Invalid credentials.");
		return await BuildAuthResponseAsync(user, ipAddress, cancellationToken);
	}

	public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, string? ipAddress,
		CancellationToken cancellationToken)
	{
		var tokenHash = refreshTokenService.HashRefreshToken(request.RefreshToken);
		var existing =
			await dbContext.RefreshTokens.Include(x => x.User)
				.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken) ??
			throw new UnauthorizedAccessException("Invalid refresh token.");

		if (existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTime.UtcNow)
			throw new UnauthorizedAccessException("Refresh token expired or revoked.");

		existing.RevokedAtUtc = DateTime.UtcNow;
		existing.RevokedByIp = ipAddress;
		return await BuildAuthResponseAsync(existing.User, ipAddress, cancellationToken, existing);
	}

	public async Task LogoutAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken)
	{
		var tokenHash = refreshTokenService.HashRefreshToken(refreshToken);
		var existing =
			await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

		if (existing is null) return;
		existing.RevokedAtUtc = DateTime.UtcNow;
		existing.RevokedByIp = ipAddress;
		await dbContext.SaveChangesAsync(cancellationToken);
	}

	public Task<ClaimsPrincipal?> GetMeAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
	{
		return Task.FromResult<ClaimsPrincipal?>(principal?.Identity?.IsAuthenticated == true ? principal : null);
	}

	private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user, string? ipAddress,
		CancellationToken cancellationToken, RefreshTokenEntity? replaced = null)
	{
		var accessToken = tokenService.CreateAccessToken(user.Id, user.Email ?? string.Empty, user.DisplayName);
		var refresh = refreshTokenService.GenerateRefreshToken();
		var refreshHash = refreshTokenService.HashRefreshToken(refresh);
		var entity = new RefreshTokenEntity
		{
			Id = Guid.NewGuid(), UserId = user.Id, TokenHash = refreshHash, CreatedAtUtc = DateTime.UtcNow,
			ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays), CreatedByIp = ipAddress
		};

		if (replaced is not null) replaced.ReplacedByTokenHash = refreshHash;
		dbContext.RefreshTokens.Add(entity);
		await dbContext.SaveChangesAsync(cancellationToken);
		return new AuthResponse
		{
			AccessToken = accessToken, RefreshToken = refresh,
			ExpiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenMinutes),
			Email = user.Email ?? string.Empty, DisplayName = user.DisplayName
		};
	}
}