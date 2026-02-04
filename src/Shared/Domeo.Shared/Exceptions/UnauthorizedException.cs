namespace Domeo.Shared.Exceptions;

/// <summary>
/// Exception thrown when user is not authenticated
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("User is not authenticated.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
