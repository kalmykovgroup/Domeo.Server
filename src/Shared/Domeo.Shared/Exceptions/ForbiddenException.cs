namespace Domeo.Shared.Exceptions;

/// <summary>
/// Exception thrown when user lacks permission to access a resource
/// </summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("Access to this resource is forbidden.")
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }
}
