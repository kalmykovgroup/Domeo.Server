namespace Projects.Contracts.DTOs.Projects;

public sealed record CreateProjectRequest(string Name, string Type, Guid ClientId, string? Notes);
