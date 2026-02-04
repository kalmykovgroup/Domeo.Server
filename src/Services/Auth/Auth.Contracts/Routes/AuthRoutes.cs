namespace Auth.Contracts.Routes;

/// <summary>
/// Route definitions for Auth.API service.
/// </summary>
public static class AuthRoutes
{
    public const string ServiceName = "auth";

    /// <summary>
    /// Cookie names for authentication
    /// </summary>
    public static class Cookies
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
    }

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
    /// HTTP headers for user context forwarding (Gateway -> Microservices)
    /// </summary>
    public static class Headers
    {
        public const string UserId = "X-User-Id";
        public const string UserRole = "X-User-Role";
    }

    /// <summary>
    /// Paths for API Gateway (full paths for YARP)
    /// </summary>
    public static class Gateway
    {
        public const string Prefix = "/api/auth";
    }

    /// <summary>
    /// External service paths (AuthCenter, Frontend)
    /// </summary>
    public static class External
    {
        /// <summary>AuthCenter OAuth authorize endpoint</summary>
        public const string AuthorizeEndpoint = "/authorize";

        /// <summary>Frontend login page for error redirects</summary>
        public const string FrontendLoginPath = "/login";
    }
}
