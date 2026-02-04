namespace Projects.Abstractions.DTOs;

public sealed record CreateProjectRequest(string Name, string Type, Guid ClientId, string? Notes);
