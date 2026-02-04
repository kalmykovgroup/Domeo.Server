namespace Auth.Contracts;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private static readonly AsyncLocal<CurrentUser?> _user = new();

    public CurrentUser? User
    {
        get => _user.Value;
        set => _user.Value = value;
    }
}
