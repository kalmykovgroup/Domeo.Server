namespace Projects.API.Contracts;

public sealed record CreateProjectRequest(string Name, string Type, Guid ClientId, string? Notes);
