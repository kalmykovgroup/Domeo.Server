using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Users.API.Entities;

public sealed class User : AuditableEntity<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Role { get; private set; } = "designer";
    public bool IsActive { get; private set; } = true;

    private User() { }

    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string role = "designer")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = true
        };
    }

    public void Update(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void UpdateRole(string role)
    {
        Role = role;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
