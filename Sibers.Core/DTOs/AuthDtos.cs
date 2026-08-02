namespace Sibers.Core.DTOs;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResultDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserInfoDto
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
