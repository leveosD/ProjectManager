using Sibers.Core.Entities;

namespace Sibers.Core.Interfaces;

public interface IDocumentRepository
{
    Task<ProjectDocument?> GetByIdAsync(int id);
    Task<List<ProjectDocument>> GetByProjectIdAsync(int projectId);
    ProjectDocument Add(ProjectDocument document);
    void Delete(ProjectDocument document);
}
