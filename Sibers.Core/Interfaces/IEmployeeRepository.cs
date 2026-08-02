using Sibers.Core.Entities;

namespace Sibers.Core.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByUserIdAsync(string userId);
    Task<List<Employee>> GetAllAsync();
    Task<List<Employee>> SearchEmployeesAsync(string? query, IEnumerable<string>? roles = null);
    void Add(Employee employee);
    void Update(Employee employee);
    void Delete(Employee employee);
}
