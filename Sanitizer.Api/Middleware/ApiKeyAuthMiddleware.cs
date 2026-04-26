using Sanitizer.Api.Services;

namespace Sanitizer.Api.Middleware;

/// <summary>
/// Проверяет заголовок x-auth-token.
/// Публичные пути: /swagger, /health — без авторизации.
/// AdminKey (из конфига) даёт доступ к управлению ключами.
/// HTTP 401 — ключ отсутствует или невалиден.
/// HTTP 403 — ключ валиден, но нет прав (не admin).
/// </summary>
public class ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration config)
{
    private static readonly string[] PublicPrefixes = ["/swagger", "/health"];

    public async Task InvokeAsync(HttpContext context, ApiKeyService apiKeyService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (PublicPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("x-auth-token", out var token) ||
            string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: x-auth-token header is required.");
            return;
        }

        var plain    = token.ToString();
        var adminKey = config["AdminKey"];
        var isAdmin  = !string.IsNullOrEmpty(adminKey) && plain == adminKey;
        var isValid  = isAdmin || await apiKeyService.IsValidAsync(plain);

        if (!isValid)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: invalid or expired API key.");
            return;
        }

        context.Items["IsAdmin"] = isAdmin;
        await next(context);
    }
}
