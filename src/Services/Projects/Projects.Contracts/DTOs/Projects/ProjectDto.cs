namespace Projects.Contracts.DTOs.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string Type,
    string Status,
    Guid ClientId,
    Guid UserId,
    string? Notes,
    string? QuestionnaireData,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt);
