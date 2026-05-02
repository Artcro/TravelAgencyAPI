using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.Auth;
using TravelAgency.Application.DTOs.Auth;
using Microsoft.AspNetCore.RateLimiting;
namespace TravelAgency.Api.Controllers;
[ApiController][Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [EnableRateLimiting("auth-strict")]
    [HttpPost("register")] public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)=>Ok(await authService.RegisterAsync(request,HttpContext.Connection.RemoteIpAddress?.ToString(),ct));
    [EnableRateLimiting("auth-strict")]
    [HttpPost("login")] public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)=>Ok(await authService.LoginAsync(request,HttpContext.Connection.RemoteIpAddress?.ToString(),ct));
    [EnableRateLimiting("auth-strict")]
    [HttpPost("refresh")] public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)=>Ok(await authService.RefreshAsync(request,HttpContext.Connection.RemoteIpAddress?.ToString(),ct));
    [HttpPost("logout")] public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct){ await authService.LogoutAsync(request.RefreshToken,HttpContext.Connection.RemoteIpAddress?.ToString(),ct); return NoContent(); }
    [HttpGet("me")] public async Task<IActionResult> Me(CancellationToken ct){ var me=await authService.GetMeAsync(User,ct); return me is null ? Unauthorized():Ok(new { user = new { name = me.Identity?.Name } }); }
}
