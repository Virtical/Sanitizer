namespace Sanitizer.Api.Auth;

/// <summary>
/// Контекст текущего API-ключа в рамках HTTP-запроса.
/// Используется сервисами для фильтрации данных по владельцу.
/// </summary>
public interface ICurrentApiKeyContext
{
    /// <summary>
    /// Идентификатор API-ключа текущего запроса.
    /// <see cref="Guid.Empty"/> — внутренний (UI/Razor) запрос вне публичного API.
    /// </summary>
    Guid ApiKeyId { get; }
}
