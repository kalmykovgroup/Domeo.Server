namespace Users.API.Contracts;

public sealed record CreateUserRequest(
    string Email,
    string PasswordHash,
    string FirstName,
    string LastName);
