using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Materials.Abstractions.Entities;

public sealed class Supplier : Entity<Guid>
{
    public string Company { get; private set; } = string.Empty;
    public string? ContactFirstName { get; private set; }
    public string? ContactLastName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Supplier() { }

    public static Supplier Create(
        Guid id,
        string company,
        string? contactFirstName = null,
        string? contactLastName = null,
        string? email = null,
        string? phone = null,
        string? address = null)
    {
        return new Supplier
        {
            Id = id,
            Company = company,
            ContactFirstName = contactFirstName,
            ContactLastName = contactLastName,
            Email = email,
            Phone = phone,
            Address = address,
            IsActive = true
        };
    }

    public void Update(string company, string? contactFirstName, string? contactLastName, string? email, string? phone, string? address)
    {
        Company = company;
        ContactFirstName = contactFirstName;
        ContactLastName = contactLastName;
        Email = email;
        Phone = phone;
        Address = address;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
