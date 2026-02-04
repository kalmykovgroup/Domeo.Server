namespace Domeo.Shared.Auth;

public interface ICurrentUserAccessor
{
    CurrentUser? User { get; set; }
}
