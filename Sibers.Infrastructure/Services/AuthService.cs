using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;

namespace Sibers.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmployeeService _employeeService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<IdentityUser> userManager,
        IEmployeeService employeeService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _employeeService = employeeService;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Id);
        if (employee == null)
        {
            throw new UnauthorizedAccessException("User account is not associated with an active employee.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Employee";

        return GenerateJwtToken(user, primaryRole, employee);
    }

    private AuthResultDto GenerateJwtToken(IdentityUser user, string role, Employee employee)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? "SuperSecretKeyForSibersProjectManagementSystem2026!";
        var issuer = jwtSettings["Issuer"] ?? "SibersAPI";
        var audience = jwtSettings["Audience"] ?? "SibersClient";
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryInMinutes"], out var mins) ? mins : 480;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, role),
            new("EmployeeId", employee.Id.ToString()),
            new("FullName", employee.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResultDto
        {
            Token = tokenString,
            Email = user.Email ?? string.Empty,
            Role = role,
            EmployeeId = employee.Id,
            FullName = employee.FullName,
            ExpiresAt = expiresAt
        };
    }
}
