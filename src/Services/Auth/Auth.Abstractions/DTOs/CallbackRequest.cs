namespace Auth.Abstractions.DTOs;

public sealed record CallbackRequest(string Code, string RedirectUri);
