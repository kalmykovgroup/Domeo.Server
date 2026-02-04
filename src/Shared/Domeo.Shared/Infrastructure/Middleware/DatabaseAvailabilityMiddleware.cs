using System.Text.Json;
using Domeo.Shared.Contracts;
using Domeo.Shared.Infrastructure.Resilience;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Domeo.Shared.Infrastructure.Middleware;

public sealed class DatabaseAvailabilityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConnectionStateTracker _stateTracker;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/openapi",
        "/scalar"
    };

    private const string ServiceUnavailableHtml = """
        <!DOCTYPE html>
        <html lang="ru">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Сервис временно недоступен</title>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    padding: 20px;
                }
                .container {
                    background: white;
                    border-radius: 16px;
                    padding: 48px;
                    max-width: 480px;
                    width: 100%;
                    text-align: center;
                    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
                }
                .icon {
                    width: 80px;
                    height: 80px;
                    margin: 0 auto 24px;
                    background: #fef3c7;
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                .icon svg {
                    width: 40px;
                    height: 40px;
                    color: #d97706;
                }
                h1 {
                    color: #1f2937;
                    font-size: 24px;
                    font-weight: 600;
                    margin-bottom: 12px;
                }
                .description {
                    color: #6b7280;
                    font-size: 16px;
                    line-height: 1.6;
                    margin-bottom: 24px;
                }
                .status {
                    background: #f3f4f6;
                    border-radius: 8px;
                    padding: 16px;
                    margin-bottom: 24px;
                }
                .status-label {
                    color: #9ca3af;
                    font-size: 12px;
                    text-transform: uppercase;
                    letter-spacing: 0.05em;
                    margin-bottom: 4px;
                }
                .status-value {
                    color: #ef4444;
                    font-weight: 500;
                }
                .status-value.checking {
                    color: #f59e0b;
                }
                .btn {
                    display: inline-block;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    font-size: 16px;
                    font-weight: 500;
                    padding: 14px 32px;
                    border: none;
                    border-radius: 8px;
                    cursor: pointer;
                    text-decoration: none;
                    transition: transform 0.2s, box-shadow 0.2s;
                }
                .btn:hover {
                    transform: translateY(-2px);
                    box-shadow: 0 10px 20px -10px rgba(102, 126, 234, 0.5);
                }
                .btn:disabled {
                    opacity: 0.7;
                    cursor: not-allowed;
                    transform: none;
                }
                .countdown {
                    color: #9ca3af;
                    font-size: 14px;
                    margin-top: 16px;
                }
            </style>
        </head>
        <body>
            <div class="container">
                <div class="icon">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                </div>
                <h1>Сервис временно недоступен</h1>
                <p class="description">
                    Мы испытываем временные технические трудности.
                    Пожалуйста, подождите несколько секунд и попробуйте снова.
                </p>
                <div class="status">
                    <div class="status-label">Статус подключения</div>
                    <div class="status-value" id="status">База данных недоступна</div>
                </div>
                <button class="btn" id="retryBtn" onclick="retry()">Попробовать снова</button>
                <div class="countdown" id="countdown">Автоматическая проверка через <span id="timer">10</span> сек.</div>
            </div>
            <script>
                let seconds = 10;
                let interval;

                function startCountdown() {
                    seconds = 10;
                    document.getElementById('timer').textContent = seconds;
                    interval = setInterval(() => {
                        seconds--;
                        document.getElementById('timer').textContent = seconds;
                        if (seconds <= 0) {
                            clearInterval(interval);
                            retry();
                        }
                    }, 1000);
                }

                function retry() {
                    clearInterval(interval);
                    document.getElementById('status').textContent = 'Проверка...';
                    document.getElementById('status').className = 'status-value checking';
                    document.getElementById('retryBtn').disabled = true;
                    document.getElementById('countdown').style.display = 'none';

                    fetch(window.location.href, {
                        headers: { 'X-Requested-With': 'XMLHttpRequest' }
                    })
                    .then(response => {
                        if (response.ok || response.status !== 503) {
                            window.location.reload();
                        } else {
                            showError();
                        }
                    })
                    .catch(() => showError());
                }

                function showError() {
                    document.getElementById('status').textContent = 'База данных недоступна';
                    document.getElementById('status').className = 'status-value';
                    document.getElementById('retryBtn').disabled = false;
                    document.getElementById('countdown').style.display = 'block';
                    startCountdown();
                }

                startCountdown();
            </script>
        </body>
        </html>
        """;

    public DatabaseAvailabilityMiddleware(RequestDelegate next, IConnectionStateTracker stateTracker)
    {
        _next = next;
        _stateTracker = stateTracker;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip check for excluded paths
        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        // If database is unavailable, return 503
        if (!_stateTracker.IsDatabaseAvailable)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers.RetryAfter = "10";

            if (ShouldReturnHtml(context.Request))
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(ServiceUnavailableHtml);
            }
            else
            {
                context.Response.ContentType = "application/json";
                var response = ApiResponse.Fail("Database temporarily unavailable");
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
            }
            return;
        }

        await _next(context);
    }

    private static bool ShouldReturnHtml(HttpRequest request)
    {
        // AJAX requests get JSON
        if (request.Headers.XRequestedWith == "XMLHttpRequest")
            return false;

        var acceptHeader = request.Headers.Accept.ToString();
        if (string.IsNullOrEmpty(acceptHeader))
            return false;

        // text/html must appear before application/json
        var htmlIndex = acceptHeader.IndexOf("text/html", StringComparison.OrdinalIgnoreCase);
        var jsonIndex = acceptHeader.IndexOf("application/json", StringComparison.OrdinalIgnoreCase);

        return htmlIndex >= 0 && (jsonIndex < 0 || htmlIndex < jsonIndex);
    }

    private static bool IsExcludedPath(string path)
    {
        foreach (var excludedPath in ExcludedPaths)
        {
            if (path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

public static class DatabaseAvailabilityMiddlewareExtensions
{
    public static IApplicationBuilder UseDatabaseAvailability(this IApplicationBuilder app)
    {
        return app.UseMiddleware<DatabaseAvailabilityMiddleware>();
    }
}
