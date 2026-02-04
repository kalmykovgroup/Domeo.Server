namespace Domeo.Shared.Kernel.Domain.Results;

/// <summary>
/// Represents an error with code and description
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided");

    public static Error NotFound(string entityName, object id) =>
        new($"{entityName}.NotFound", $"{entityName} with id '{id}' was not found");

    public static Error NotFound(string message) =>
        new("NotFound", message);

    public static Error ServiceUnavailable(string message) =>
        new("ServiceUnavailable", message);

    public static Error Validation(string code, string description) =>
        new(code, description);

    public static Error Conflict(string code, string description) =>
        new(code, description);

    public static Error Failure(string code, string description) =>
        new(code, description);
}
