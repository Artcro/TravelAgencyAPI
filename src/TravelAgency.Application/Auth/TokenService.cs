using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TravelAgency.Application.Config;

namespace TravelAgency.Application.Auth;

public sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
	public string CreateAccessToken(Guid userId, string email, string displayName)
	{
		var jwt = options.Value;
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), new Claim(JwtRegisteredClaimNames.Email, email),
			new Claim("displayName", displayName)
		};

		var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims,
			expires: DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes), signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}