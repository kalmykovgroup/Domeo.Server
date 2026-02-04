namespace Auth.Contracts.DTOs;

public sealed record CallbackRequest(string Code, string RedirectUri);
