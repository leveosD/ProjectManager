namespace Sibers.Core.Interfaces;

public interface ICurrentUserService
{
    int? EmployeeId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}