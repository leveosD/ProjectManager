using Microsoft.EntityFrameworkCore;
using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Enums;
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
            "startdate" => filter.SortDescending
                ? query.OrderByDescending(p => p.StartDate)
                : query.OrderBy(p => p.StartDate),
            "enddate" => filter.SortDescending
                ? query.OrderByDescending(p => p.EndDate)
                : query.OrderBy(p => p.EndDate),
            "priority" => filter.SortDescending
                ? query.OrderByDescending(p => p.Priority)
                : query.OrderBy(p => p.Priority),
            _ => query.OrderByDescending(p => p.Priority) // Default sort by Priority descending
        };

        return await query.ToListAsync();
    }

    public void Add(Project project)
    {
        _context.Projects.Add(project);
    }

    public void Update(Project project)
    {
        _context.Projects.Update(project);
    }

    public void Delete(Project project)
    {
        _context.Projects.Remove(project);
    }

    public void SetProjectEmployees(int projectId, List<int> employeeIds)
    {
        var existingRelations = _context.ProjectEmployees
            .Where(pe => pe.ProjectId == projectId)
            .ToList();

        _context.ProjectEmployees.RemoveRange(existingRelations);

        var newRelations = employeeIds.Distinct().Select(empId => new ProjectEmployee
        {
            ProjectId = projectId,
            EmployeeId = empId
        });

        _context.ProjectEmployees.AddRange(newRelations);
    }
}