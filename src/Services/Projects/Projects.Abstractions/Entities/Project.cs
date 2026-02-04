using Domeo.Shared.Domain;

namespace Projects.Abstractions.Entities;

public sealed class Project : AuditableEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public ProjectStatus Status { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Notes { get; private set; }
    public string? QuestionnaireData { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Project() { }

    public static Project Create(
        string name,
        string type,
        Guid clientId,
        Guid userId,
        string? notes = null)
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Status = ProjectStatus.Draft,
            ClientId = clientId,
            UserId = userId,
            Notes = notes
        };
    }

    public void Update(string name, string? notes)
    {
        Name = name;
        Notes = notes;
    }

    public void UpdateType(string type) => Type = type;
    public void UpdateStatus(ProjectStatus status) => Status = status;
    public void SetQuestionnaireData(string? data) => QuestionnaireData = data;

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
    }

    public bool IsDeleted => DeletedAt.HasValue;
}
