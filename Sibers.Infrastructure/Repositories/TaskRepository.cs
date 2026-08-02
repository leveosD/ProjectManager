using Microsoft.EntityFrameworkCore;
using Sibers.Core.DTOs;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Infrastructure.Data;

namespace Sibers.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectTask?> GetByIdAsync(int id)
    {
        return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<ProjectTask?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Author)
            .Include(t => t.Executor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<ProjectTask>> GetFilteredTasksAsync(ProjectTaskFilterDto filter)
    {
        IQueryable<ProjectTask> query = _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Author)
            .Include(t => t.Executor);
        
        if (filter.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == filter.ProjectId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.AuthorId.HasValue)
        {
            query = query.Where(t => t.AuthorId == filter.AuthorId.Value);
        }

        if (filter.ExecutorId.HasValue)
        {
            query = query.Where(t => t.ExecutorId == filter.ExecutorId.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(search) ||
                                     (t.Comment != null && t.Comment.ToLower().Contains(search)));
        }

        // Sorting
        query = (filter.SortBy?.ToLower()) switch
        {
            "title" => filter.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => filter.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => filter.SortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "projectname" => filter.SortDescending
                ? query.OrderByDescending(t => t.Project.Name)
                : query.OrderBy(t => t.Project.Name),
            "authorname" => filter.SortDescending
                ? query.OrderByDescending(t => t.Author.LastName).ThenByDescending(t => t.Author.FirstName)
                : query.OrderBy(t => t.Author.LastName).ThenBy(t => t.Author.FirstName),
            "executorname" => filter.SortDescending
                ? query.OrderByDescending(t => t.Executor.LastName).ThenByDescending(t => t.Executor.FirstName)
                : query.OrderBy(t => t.Executor.LastName).ThenBy(t => t.Executor.FirstName),
            _ => query.OrderByDescending(t => t.Priority) // Default sort by Priority descending
        };

        return await query.ToListAsync();
    }

    public void Add(ProjectTask task) => _context.Tasks.Add(task);

    public void Update(ProjectTask task) => _context.Tasks.Update(task);

    public void Delete(ProjectTask task) => _context.Tasks.Remove(task);
}
