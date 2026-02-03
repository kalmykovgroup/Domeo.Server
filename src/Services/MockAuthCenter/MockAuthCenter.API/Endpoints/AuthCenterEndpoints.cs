using System.Web;
using Microsoft.AspNetCore.Mvc;
using MockAuthCenter.API.Services;

namespace MockAuthCenter.API.Endpoints;

public static class AuthCenterEndpoints
{
    public static void MapAuthCenterEndpoints(this WebApplication app)
    {
        // OAuth 2.0 Authorization endpoint
        app.MapGet("/authorize", HandleAuthorize);
        app.MapPost("/authorize", HandleAuthorizePost);

        // OAuth 2.0 Token endpoint
        app.MapPost("/token", HandleToken);

        // JWKS endpoint
        app.MapGet("/.well-known/jwks.json", HandleJwks);
        app.MapGet("/.well-known/openid-configuration", HandleOpenIdConfig);

        // Logout
        app.MapPost("/logout", HandleLogout);
        app.MapGet("/logout", HandleLogoutGet);
    }

    private static IResult HandleAuthorize(
        HttpContext context,
        [FromQuery] string response_type,
        [FromQuery] string client_id,
        [FromQuery] string redirect_uri,
        [FromQuery] string? state,
        [FromQuery] string? scope)
    {
        if (response_type != "code")
        {
            return Results.BadRequest(new { error = "unsupported_response_type" });
        }

        // Return login page
        var loginHtml = GetLoginPage(client_id, redirect_uri, state);
        return Results.Content(loginHtml, "text/html");
    }

    private static async Task<IResult> HandleAuthorizePost(
        HttpContext context,
        UserStore userStore,
        AuthCodeStore authCodeStore)
    {
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString();
        var password = form["password"].ToString();
        var clientId = form["client_id"].ToString();
        var redirectUri = form["redirect_uri"].ToString();
        var state = form["state"].ToString();

        if (!userStore.ValidateCredentials(email, password, out var user))
        {
            var errorHtml = GetLoginPage(clientId, redirectUri, state, "Invalid email or password");
            return Results.Content(errorHtml, "text/html");
        }

        // Check if user has access to this client/program
        var role = userStore.GetRoleForProgram(user!, clientId);
        if (role == null)
        {
            var errorHtml = GetLoginPage(clientId, redirectUri, state, "You don't have access to this application");
            return Results.Content(errorHtml, "text/html");
        }

        // Create authorization code
        var code = authCodeStore.CreateCode(user!.Id, clientId, redirectUri, state);

        // Redirect back to client
        var redirectUrl = $"{redirectUri}?code={code}";
        if (!string.IsNullOrEmpty(state))
        {
            redirectUrl += $"&state={HttpUtility.UrlEncode(state)}";
        }

        return Results.Redirect(redirectUrl);
    }

    private static async Task<IResult> HandleToken(
        HttpContext context,
        UserStore userStore,
        AuthCodeStore authCodeStore,
        RefreshTokenStore refreshTokenStore,
        TokenService tokenService)
    {
        var form = await context.Request.ReadFormAsync();
        var grantType = form["grant_type"].ToString();

        return grantType switch
        {
            "authorization_code" => HandleAuthorizationCodeGrant(
                form, userStore, authCodeStore, refreshTokenStore, tokenService),
            "refresh_token" => HandleRefreshTokenGrant(
                form, userStore, refreshTokenStore, tokenService),
            _ => Results.BadRequest(new { error = "unsupported_grant_type" })
        };
    }

    private static IResult HandleAuthorizationCodeGrant(
        IFormCollection form,
        UserStore userStore,
        AuthCodeStore authCodeStore,
        RefreshTokenStore refreshTokenStore,
        TokenService tokenService)
    {
        var code = form["code"].ToString();
        var redirectUri = form["redirect_uri"].ToString();
        var clientId = form["client_id"].ToString();

        if (string.IsNullOrEmpty(code))
        {
            return Results.BadRequest(new { error = "invalid_request", error_description = "code is required" });
        }

        var authCode = authCodeStore.ConsumeCode(code);
        if (authCode == null)
        {
            return Results.BadRequest(new { error = "invalid_grant", error_description = "Invalid or expired code" });
        }

        if (authCode.RedirectUri != redirectUri)
        {
            return Results.BadRequest(new { error = "invalid_grant", error_description = "redirect_uri mismatch" });
        }

        if (authCode.ClientId != clientId)
        {
            return Results.BadRequest(new { error = "invalid_grant", error_description = "client_id mismatch" });
        }

        var user = userStore.FindById(authCode.UserId);
        if (user == null)
        {
            return Results.BadRequest(new { error = "invalid_grant", error_description = "User not found" });
        }

        var refreshToken = refreshTokenStore.CreateToken(user.Id, clientId);
        var response = tokenService.CreateTokenResponse(user, clientId, refreshToken);

        return Results.Ok(response);
    }

    private static IResult HandleRefreshTokenGrant(
        IFormCollection form,
        UserStore userStore,
        RefreshTokenStore refreshTokenStore,
        TokenService tokenService)
    {
        var refreshToken = form["refresh_token"].ToString();
        var clientId = form["client_id"].ToString();

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.BadRequest(new { error = "invalid_request", error_description = "refresh_token is required" });
        }

        var tokenEntry = refreshTokenStore.GetToken(refreshToken);
        if (tokenEntry == null)
        {
            return Results.BadRequest(new { error = "invalid_grant", error_description = "Invalid or expired refresh token" });
        }

        if (tokenEntry.ClientId != clientId)
        {
            return Results.BadRequest(new { error = "invalid_grant", error_description = "client_id mismatch" });
        }

        var user = userStore.FindById(tokenEntry.UserId);
        if (user == null)
        {
            return Results.BadRequest(new { error = "invalid_grant", error_description = "User not found" });
        }

        // Rotate refresh token
        var newRefreshToken = refreshTokenStore.RotateToken(refreshToken, user.Id, clientId);
        var response = tokenService.CreateTokenResponse(user, clientId, newRefreshToken);

        return Results.Ok(response);
    }

    private static IResult HandleJwks(RsaKeyService rsaKeyService)
    {
        return Results.Ok(rsaKeyService.GetJwks());
    }

    private static IResult HandleOpenIdConfig(HttpContext context, IConfiguration configuration)
    {
        var issuer = configuration["AuthCenter:Issuer"] ?? "http://localhost:5100";

        return Results.Ok(new
        {
            issuer,
            authorization_endpoint = $"{issuer}/authorize",
            token_endpoint = $"{issuer}/token",
            jwks_uri = $"{issuer}/.well-known/jwks.json",
            response_types_supported = new[] { "code" },
            grant_types_supported = new[] { "authorization_code", "refresh_token" },
            token_endpoint_auth_methods_supported = new[] { "client_secret_post", "none" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" }
        });
    }

    private static IResult HandleLogout(
        HttpContext context,
        RefreshTokenStore refreshTokenStore,
        [FromQuery] string? refresh_token,
        [FromQuery] string? post_logout_redirect_uri)
    {
        if (!string.IsNullOrEmpty(refresh_token))
        {
            refreshTokenStore.RevokeToken(refresh_token);
        }

        if (!string.IsNullOrEmpty(post_logout_redirect_uri))
        {
            return Results.Redirect(post_logout_redirect_uri);
        }

        return Results.Ok(new { message = "Logged out successfully" });
    }

    private static IResult HandleLogoutGet(
        [FromQuery] string? post_logout_redirect_uri)
    {
        if (!string.IsNullOrEmpty(post_logout_redirect_uri))
        {
            return Results.Redirect(post_logout_redirect_uri);
        }

        return Results.Content(GetLogoutPage(), "text/html");
    }

    private static string GetLoginPage(string clientId, string redirectUri, string? state, string? error = null)
    {
        var errorHtml = error != null
            ? $"<div class=\"error\">{HttpUtility.HtmlEncode(error)}</div>"
            : "";

        var html = LoginPageTemplate
            .Replace("{{CLIENT_ID}}", HttpUtility.HtmlEncode(clientId))
            .Replace("{{CLIENT_ID_ATTR}}", HttpUtility.HtmlAttributeEncode(clientId))
            .Replace("{{REDIRECT_URI}}", HttpUtility.HtmlAttributeEncode(redirectUri))
            .Replace("{{STATE}}", HttpUtility.HtmlAttributeEncode(state ?? ""))
            .Replace("{{ERROR_HTML}}", errorHtml);

        return html;
    }

    private static string GetLogoutPage()
    {
        return LogoutPageTemplate;
    }

    private const string LoginPageTemplate = """
<!DOCTYPE html>
<html>
<head>
    <title>Mock Auth Center - Login</title>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <style>
        * { box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0;
            padding: 20px;
        }
        .container {
            background: white;
            padding: 40px;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            width: 100%;
            max-width: 400px;
        }
        h1 {
            margin: 0 0 8px 0;
            color: #333;
            font-size: 24px;
            text-align: center;
        }
        .subtitle {
            color: #666;
            text-align: center;
            margin-bottom: 30px;
            font-size: 14px;
        }
        .client-info {
            background: #f5f5f5;
            padding: 12px;
            border-radius: 8px;
            margin-bottom: 24px;
            font-size: 13px;
            color: #666;
        }
        .form-group {
            margin-bottom: 20px;
        }
        label {
            display: block;
            margin-bottom: 8px;
            color: #333;
            font-weight: 500;
            font-size: 14px;
        }
        input {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e1e1e1;
            border-radius: 8px;
            font-size: 16px;
            transition: border-color 0.2s;
        }
        input:focus {
            outline: none;
            border-color: #667eea;
        }
        button {
            width: 100%;
            padding: 14px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s, box-shadow 0.2s;
        }
        button:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
        }
        .error {
            background: #fee;
            color: #c00;
            padding: 12px;
            border-radius: 8px;
            margin-bottom: 20px;
            font-size: 14px;
        }
        .test-users {
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
        }
        .test-users h3 {
            font-size: 13px;
            color: #666;
            margin: 0 0 12px 0;
        }
        .test-user {
            background: #f9f9f9;
            padding: 10px 12px;
            border-radius: 6px;
            margin-bottom: 8px;
            font-size: 12px;
            cursor: pointer;
            transition: background 0.2s;
        }
        .test-user:hover {
            background: #f0f0f0;
        }
        .test-user strong {
            color: #333;
        }
        .test-user span {
            color: #888;
            margin-left: 8px;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>Mock Auth Center</h1>
        <p class="subtitle">Development Authentication Server</p>

        <div class="client-info">
            Signing in to: <strong>{{CLIENT_ID}}</strong>
        </div>

        {{ERROR_HTML}}

        <form method="POST" action="/authorize">
            <input type="hidden" name="client_id" value="{{CLIENT_ID_ATTR}}">
            <input type="hidden" name="redirect_uri" value="{{REDIRECT_URI}}">
            <input type="hidden" name="state" value="{{STATE}}">

            <div class="form-group">
                <label for="email">Email</label>
                <input type="email" id="email" name="email" required autocomplete="email">
            </div>

            <div class="form-group">
                <label for="password">Password</label>
                <input type="password" id="password" name="password" required autocomplete="current-password">
            </div>

            <button type="submit">Sign In</button>
        </form>

        <div class="test-users">
            <h3>Test Accounts (click to fill)</h3>
            <div class="test-user" onclick="fillCredentials('sales@test.com', 'sales123')">
                <strong>sales@test.com</strong> / sales123 <span>Комплектатор</span>
            </div>
            <div class="test-user" onclick="fillCredentials('designer@test.com', 'designer123')">
                <strong>designer@test.com</strong> / designer123 <span>Проектировщик</span>
            </div>
            <div class="test-user" onclick="fillCredentials('catalog@test.com', 'catalog123')">
                <strong>catalog@test.com</strong> / catalog123 <span>Админ каталога</span>
            </div>
            <div class="test-user" onclick="fillCredentials('admin@test.com', 'admin123')">
                <strong>admin@test.com</strong> / admin123 <span>Системный админ</span>
            </div>
        </div>
    </div>

    <script>
        function fillCredentials(email, password) {
            document.getElementById('email').value = email;
            document.getElementById('password').value = password;
        }
    </script>
</body>
</html>
""";

    private const string LogoutPageTemplate = """
<!DOCTYPE html>
<html>
<head>
    <title>Mock Auth Center - Logged Out</title>
    <meta charset="UTF-8">
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0;
        }
        .container {
            background: white;
            padding: 40px;
            border-radius: 12px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            text-align: center;
        }
        h1 { color: #333; margin-bottom: 16px; }
        p { color: #666; }
    </style>
</head>
<body>
    <div class="container">
        <h1>Logged Out</h1>
        <p>You have been successfully logged out.</p>
    </div>
</body>
</html>
""";
}
