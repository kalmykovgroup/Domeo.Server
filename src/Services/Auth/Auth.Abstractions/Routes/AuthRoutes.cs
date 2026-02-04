namespace Auth.Abstractions.Routes;

/// <summary>
/// Route definitions for Auth.API service.
/// </summary>
public static class AuthRoutes
{
    public const string ServiceName = "auth";

    /// <summary>
    /// Paths for controllers (relative, used in [Route] and [HttpGet])
    /// </summary>
    public static class Controller
    {
        public const string Base = "auth";
        public const string Login = "login";
        public const string Callback = "callback";
        public const string Refresh = "refresh";
        public const string Logout = "logout";
        public const string Me = "me";
        public const string Token = "token";
    }

    /// <summary>
    /// Paths for API Gateway (full paths for YARP)
    /// </summary>
    public static class Gateway
    {
        public const string Prefix = "/api/auth";
    }
}
