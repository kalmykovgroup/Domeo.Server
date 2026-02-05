using Projects.Infrastructure.Persistence;

namespace Projects.API.Services;

public sealed class ProjectsSeeder
{
    private readonly ProjectsDbContext _dbContext;
    private readonly ILogger<ProjectsSeeder> _logger;

    public ProjectsSeeder(ProjectsDbContext dbContext, ILogger<ProjectsSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Projects database ready (no seed data)");
        await Task.CompletedTask;
    }
}
