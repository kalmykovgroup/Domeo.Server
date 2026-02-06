using Domeo.Shared.Domain;

namespace Modules.Domain.Entities;

public sealed class StorageConnection : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = "s3";
    public string Endpoint { get; private set; } = string.Empty;
    public string Bucket { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public string AccessKey { get; private set; } = string.Empty;
    public string SecretKey { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private StorageConnection() { }

    public static StorageConnection Create(
        string name,
        string endpoint,
        string bucket,
        string region,
        string accessKey,
        string secretKey)
    {
        return new StorageConnection
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = "s3",
            Endpoint = endpoint,
            Bucket = bucket,
            Region = region,
            AccessKey = accessKey,
            SecretKey = secretKey,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string endpoint,
        string bucket,
        string region,
        string accessKey,
        string secretKey,
        bool isActive)
    {
        Name = name;
        Endpoint = endpoint;
        Bucket = bucket;
        Region = region;
        AccessKey = accessKey;
        SecretKey = secretKey;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
