namespace Auth.Contracts;

public sealed class CurrentUser
{
    public Guid? Id { get; set; }
    public string Role { get; set; } = string.Empty;
}
