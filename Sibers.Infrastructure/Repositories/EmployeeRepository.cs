using Microsoft.EntityFrameworkCore;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Infrastructure.Data;

namespace Sibers.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee?> GetByUserIdAsync(string userId)
    {
        return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<List<Employee>> SearchEmployeesAsync(string? query, IEnumerable<string>? roles = null)
    {
        IQueryable<Employee> employees = _context.Employees;

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim().ToLower();

            employees = employees.Where(e => e.FirstName.ToLower().Contains(normalizedQuery) ||
                                             e.LastName.ToLower().Contains(normalizedQuery) ||
                                             (e.MiddleName != null && e.MiddleName.ToLower().Contains(normalizedQuery)) ||
                                             e.Email.ToLower().Contains(normalizedQuery));
        }

        employees = ApplyRoleFilter(employees, roles);

        return await employees
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public void Add(Employee employee)
    {
        _context.Employees.Add(employee);
    }

    public void Update(Employee employee)
    {
        _context.Employees.Update(employee);
    }

    public void Delete(Employee employee)
    {
        _context.Employees.Remove(employee);
    }

    private IQueryable<Employee> ApplyRoleFilter(IQueryable<Employee> query, IEnumerable<string>? roles)
    {
        if (roles == null)
        {
            return query;
        }

        var roleList = roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .ToList();

        if (roleList.Count == 0)
        {
            return query;
        }

        var userIdsInRoles = _context.UserRoles
            .Join(
                _context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, RoleName = r.Name })
            .Where(x => x.RoleName != null && roleList.Contains(x.RoleName))
            .Select(x => x.UserId);

        return query.Where(e => e.UserId != null && userIdsInRoles.Contains(e.UserId));
    }
}
