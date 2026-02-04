namespace Domeo.Shared.Exceptions;

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate resource)
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string resourceType, string conflictReason)
        : base($"{resourceType} conflict: {conflictReason}")
    {
    }
}
