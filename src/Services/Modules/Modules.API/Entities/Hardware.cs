using Domeo.Shared.Kernel.Domain.Abstractions;

namespace Modules.API.Entities;

public sealed class Hardware : Entity<int>
{
    public string Type { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public string? ModelUrl { get; private set; }
    public string? Params { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Hardware() { }

    public static Hardware Create(
        int id,
        string type,
        string name,
        string? brand = null,
        string? model = null,
        string? modelUrl = null,
        string? @params = null)
    {
        return new Hardware
        {
            Id = id,
            Type = type,
            Name = name,
            Brand = brand,
            Model = model,
            ModelUrl = modelUrl,
            Params = @params,
            IsActive = true
        };
    }

    public void Update(string name, string? modelUrl, string? @params)
    {
        Name = name;
        ModelUrl = modelUrl;
        Params = @params;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
