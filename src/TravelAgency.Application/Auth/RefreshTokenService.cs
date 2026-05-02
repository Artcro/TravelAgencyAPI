using System.Security.Cryptography; using System.Text;
namespace TravelAgency.Application.Auth;
public sealed class RefreshTokenService : IRefreshTokenService { public string GenerateRefreshToken()=>Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)); public string HashRefreshToken(string token)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))); }
