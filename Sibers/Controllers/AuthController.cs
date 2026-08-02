using Microsoft.AspNetCore.Mvc;
using Sibers.Core.DTOs;
using Sibers.Core.Interfaces;

namespace Sibers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserInfoDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = response.ExpiresAt
            };

            Response.Cookies.Append("authToken", response.Token, cookieOptions);

            return Ok(new UserInfoDto
            {
                Email = response.Email,
                Role = response.Role,
                EmployeeId = response.EmployeeId,
                FullName = response.FullName,
                ExpiresAt = response.ExpiresAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("authToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
        return Ok(new { message = "Logged out successfully" });
    }
}
