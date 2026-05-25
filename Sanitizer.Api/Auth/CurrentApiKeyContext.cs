namespace Sanitizer.Api.Auth;

/// <summary>
/// Scoped-реализация контекста текущего API-ключа.
/// Значение <see cref="ApiKeyId"/> устанавливается middleware аутентификации
/// (<c>ApiKeyAuthMiddleware</c>) для запросов к публичному API.
/// Для внутренних (UI/Razor) запросов остаётся <see cref="Guid.Empty"/>.
/// </summary>
public sealed class CurrentApiKeyContext : ICurrentApiKeyContext
{
    /// <inheritdoc />
    public Guid ApiKeyId { get; private set; } = Guid.Empty;

    /// <summary>
    /// Устанавливает идентификатор API-ключа для текущего запроса.
    /// Должен вызываться один раз из middleware аутентификации.
    /// </summary>
    /// <param name="apiKeyId">Идентификатор аутентифицированного API-ключа.</param>
    public void SetApiKeyId(Guid apiKeyId)
    {
        ApiKeyId = apiKeyId;
    }
}
