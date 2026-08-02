namespace Sibers.Core.Interfaces;

/// <summary>
/// Provides domain-oriented operations for managing employee accounts.
/// Implementation lives in Infrastructure to keep Core free of Identity dependencies.
/// </summary>
public interface IEmployeeAccountService
{
    /// <summary>
    /// Creates an identity account for the specified employee and assigns a role.
    /// </summary>
    /// <param name="employeeId">Employee identifier.</param>
    /// <param name="email">Account email (used as user name).</param>
    /// <param name="password">Account password.</param>
    /// <param name="role">Desired role. If null or empty, the default employee role is used.</param>
    Task<string> CreateAccountAsync(string email, string password, string? role);

    /// <summary>
    /// Deletes the identity account associated with the specified employee.
    /// </summary>
    Task DeleteAccountByUserIdAsync(string? userId);

    /// <summary>
    /// Returns the primary role assigned to the employee's account, if any.
    /// </summary>
    Task<string?> GetRoleByUserIdAsync(string? userId);

    /// <summary>
    /// Sets the primary role for the employee's account.
    /// </summary>
    Task SetRoleByUserIdAsync(string? userId, string role);
}
