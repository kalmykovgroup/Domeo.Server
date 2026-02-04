namespace Domeo.Shared.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public sealed class NotFoundException : Exception
{
    public string? ResourceType { get; }
    public string? ResourceId { get; }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with id '{resourceId}' was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId.ToString();
    }
}
