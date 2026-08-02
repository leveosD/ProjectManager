using Microsoft.EntityFrameworkCore;
using Sibers.Core.Entities;
using Sibers.Core.Interfaces;
using Sibers.Infrastructure.Data;

namespace Sibers.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDocument?> GetByIdAsync(int id)
    {
        return await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<ProjectDocument>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Documents
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public ProjectDocument Add(ProjectDocument document)
    {
        _context.Documents.Add(document);
        return  document;
    }

    public void Delete(ProjectDocument document)
    {
        _context.Documents.Remove(document);
    }
}
