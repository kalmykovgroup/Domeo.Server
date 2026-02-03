using System.Text.Json.Serialization;

namespace Domeo.Shared.Contracts;

public sealed class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = [];

    [JsonConstructor]
    public ApiResponse() { }

    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Fail(string error) => new()
    {
        Success = false,
        Errors = [error]
    };

    public static ApiResponse Fail(List<string> errors) => new()
    {
        Success = false,
        Errors = errors
    };
}
