using Domeo.Shared.Domain;

namespace Modules.Domain.Entities;

/// <summary>
/// Подключение к S3-совместимому хранилищу для загрузки GLB-моделей и других файлов.
/// </summary>
public sealed class StorageConnection : Entity<Guid>
{
    /// <summary>Отображаемое название подключения.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Тип хранилища (на данный момент всегда <c>"s3"</c>).</summary>
    public string Type { get; private set; } = "s3";

    /// <summary>URL эндпоинта S3-совместимого хранилища.</summary>
    public string Endpoint { get; private set; } = string.Empty;

    /// <summary>Имя бакета (bucket) для хранения файлов.</summary>
    public string Bucket { get; private set; } = string.Empty;

    /// <summary>Регион хранилища (например, <c>"ru-central1"</c>).</summary>
    public string Region { get; private set; } = string.Empty;

    /// <summary>Ключ доступа (Access Key ID) для аутентификации в хранилище.</summary>
    public string AccessKey { get; private set; } = string.Empty;

    /// <summary>Секретный ключ (Secret Access Key) для аутентификации в хранилище.</summary>
    public string SecretKey { get; private set; } = string.Empty;

    /// <summary>Флаг активности подключения. Только одно подключение может быть активным.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Дата и время создания (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Дата и время последнего обновления (UTC). <c>null</c> — если не обновлялось.</summary>
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
