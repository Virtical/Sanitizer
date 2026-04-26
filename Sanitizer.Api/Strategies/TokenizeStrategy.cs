using Sanitizer.Api.Models;
using Sanitizer.Api.Services;

namespace Sanitizer.Api.Strategies;

/// <summary>
/// Обратимая токенизация: [EMAIL_1], [PHONE_2] и т.д.
/// Одинаковые значения в одной сессии получают одинаковый токен.
/// </summary>
public class TokenizeStrategy(TokenStore tokenStore) : ISanitizationStrategy
{
    public string Apply(string value, DetectorType type, Dictionary<string, string> parameters, string sessionId)
    {
        var ttl = int.Parse(parameters.GetValueOrDefault("ttlSeconds", "3600"));
        return tokenStore.GetOrCreate(sessionId, value, type, ttl);
    }
}
