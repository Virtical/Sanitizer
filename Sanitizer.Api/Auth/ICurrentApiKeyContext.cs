namespace Sanitizer.Api.Auth;

/// <summary>
/// Контекст текущего API-ключа в рамках HTTP-запроса.
/// Используется сервисами для фильтрации данных по владельцу.
/// </summary>
public interface ICurrentApiKeyContext
{
    /// <summary>
    /// Идентификатор API-ключа текущего запроса.
    /// </summary>
    Guid ApiKeyId { get; }
}
