namespace Domeo.Shared.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public sealed class DomeoValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public DomeoValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.AsReadOnly();
    }

    public DomeoValidationException(string propertyName, string errorMessage)
        : base(errorMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            [propertyName] = [errorMessage]
        }.AsReadOnly();
    }

    public DomeoValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>
        {
            [""] = [message]
        }.AsReadOnly();
    }
}
