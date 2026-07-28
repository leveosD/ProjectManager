using Sibers.Core.Entities;

namespace Sibers.Core.Interfaces;

public interface IDocumentRepository
{
    Task<ProjectDocument?> GetByIdAsync(int id);
    Task<List<ProjectDocument>> GetByProjectIdAsync(int projectId);
    Task<ProjectDocument> AddAsync(ProjectDocument document);
    Task DeleteAsync(ProjectDocument document);
}
