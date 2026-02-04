namespace Auth.Contracts;

public interface ICurrentUserAccessor
{
    CurrentUser? User { get; set; }
}
