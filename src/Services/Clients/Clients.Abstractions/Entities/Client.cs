using Domeo.Shared.Domain;

namespace Clients.Abstractions.Entities;

public sealed class Client : AuditableEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? Notes { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Client() { }

    public static Client Create(
        string name,
        Guid userId,
        string? phone = null,
        string? email = null,
        string? address = null,
        string? notes = null)
    {
        return new Client
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = userId,
            Phone = phone,
            Email = email,
            Address = address,
            Notes = notes
        };
    }

    public void Update(string name, string? phone, string? email, string? address, string? notes)
    {
        Name = name;
        Phone = phone;
        Email = email;
        Address = address;
        Notes = notes;
    }

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
