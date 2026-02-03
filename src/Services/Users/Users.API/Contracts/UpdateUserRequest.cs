namespace Users.API.Contracts;

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName);
