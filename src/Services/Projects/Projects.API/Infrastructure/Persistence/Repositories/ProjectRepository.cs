using Microsoft.EntityFrameworkCore;
using Projects.Abstractions.Entities;
using Projects.Abstractions.Repositories;

namespace Projects.API.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly ProjectsDbContext _dbContext;

    public ProjectRepository(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Project>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.Projects.ToListAsync(ct);

    public void Add(Project entity) => _dbContext.Projects.Add(entity);
    public void Update(Project entity) => _dbContext.Projects.Update(entity);
    public void Remove(Project entity) => _dbContext.Projects.Remove(entity);

    public async Task<(List<Project> Items, int Total)> GetProjectsAsync(
        Guid userId,
        Guid? clientId,
        string? search,
        ProjectStatus? status,
        string? type,
        int page,
        int pageSize,
        string? sortBy,
        bool sortDesc,
        CancellationToken ct = default)
    {
        var query = _dbContext.Projects
            .Where(p => p.UserId == userId && p.DeletedAt == null);

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(p => p.Type == type);

        query = (sortBy?.ToLower(), sortDesc) switch
        {
            ("name", true) => query.OrderByDescending(p => p.Name),
            ("name", false) => query.OrderBy(p => p.Name),
            ("createdat", true) => query.OrderByDescending(p => p.CreatedAt),
            ("createdat", false) => query.OrderBy(p => p.CreatedAt),
            ("updatedat", true) => query.OrderByDescending(p => p.UpdatedAt),
            ("updatedat", false) => query.OrderBy(p => p.UpdatedAt),
            ("status", true) => query.OrderByDescending(p => p.Status),
            ("status", false) => query.OrderBy(p => p.Status),
            _ => query.OrderByDescending(p => p.UpdatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
