using Projects.Abstractions.Entities;

namespace Projects.Abstractions.Repositories;

public interface IProjectRepository : IRepository<Project, Guid>
{
    Task<(List<Project> Items, int Total)> GetProjectsAsync(
        Guid userId,
        Guid? clientId,
        string? search,
        ProjectStatus? status,
        string? type,
        int page,
        int pageSize,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct = default);
}
