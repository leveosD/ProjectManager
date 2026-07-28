using Microsoft.EntityFrameworkCore;
using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Infrastructure.Data;

namespace Sibers.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.ProjectManager)
            .Include(p => p.ProjectEmployees)
                .ThenInclude(pe => pe.Employee)
            .Include(p => p.Documents)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Project>> GetFilteredProjectsAsync(ProjectFilterDto filter)
    {
        IQueryable<Project> query = _context.Projects
            .Include(p => p.ProjectManager)
            .Include(p => p.ProjectEmployees)
                .ThenInclude(pe => pe.Employee)
            .Include(p => p.Documents)
            .Include(p => p.Tasks);

        // Date range filtering
        if (filter.StartDateFrom.HasValue)
        {
            query = query.Where(p => p.StartDate >= filter.StartDateFrom.Value);
        }

        if (filter.StartDateTo.HasValue)
        {
            query = query.Where(p => p.StartDate <= filter.StartDateTo.Value);
        }

        // Priority filtering
        if (filter.PriorityMin.HasValue)
        {
            query = query.Where(p => p.Priority >= filter.PriorityMin.Value);
        }

        if (filter.PriorityMax.HasValue)
        {
            query = query.Where(p => p.Priority <= filter.PriorityMax.Value);
        }

        // Search term filtering
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) ||
                                     p.CustomerCompany.ToLower().Contains(search) ||
                                     p.ExecutingCompany.ToLower().Contains(search));
        }

        // Sorting
        query = (filter.SortBy?.ToLower()) switch
        {
            "name" => filter.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "startdate" => filter.SortDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
            "enddate" => filter.SortDescending ? query.OrderByDescending(p => p.EndDate) : query.OrderBy(p => p.EndDate),
            "priority" => filter.SortDescending ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority),
            _ => query.OrderByDescending(p => p.Priority) // Default sort by Priority descending
        };

        return await query.ToListAsync();
    }

    public async Task<Project> AddAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    public async Task SetProjectEmployeesAsync(int projectId, List<int> employeeIds)
    {
        var existingRelations = await _context.ProjectEmployees
            .Where(pe => pe.ProjectId == projectId)
            .ToListAsync();

        _context.ProjectEmployees.RemoveRange(existingRelations);

        var newRelations = employeeIds.Distinct().Select(empId => new ProjectEmployee
        {
            ProjectId = projectId,
            EmployeeId = empId
        });

        _context.ProjectEmployees.AddRange(newRelations);
        await _context.SaveChangesAsync();
    }
}
