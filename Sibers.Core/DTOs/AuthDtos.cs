using System.ComponentModel.DataAnnotations;

namespace Sibers.Core.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
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
