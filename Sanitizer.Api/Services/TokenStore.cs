using System.Collections.Concurrent;
using Sanitizer.Api.Models;

namespace Sanitizer.Api.Services;

/// <summary>
/// Thread-safe хранилище токенов с поддержкой TTL.
/// Одинаковые значения в одной сессии получают одинаковый токен.
/// Разные значения всегда получают разные токены.
/// </summary>
public class TokenStore
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new();

    private sealed class Session
    {
        public DateTime ExpiresAt { get; set; }
        public ConcurrentDictionary<string, string> TokenToValue { get; } = new();
        public ConcurrentDictionary<string, string> ValueToToken { get; } = new();
        public ConcurrentDictionary<DetectorType, int> Counters { get; } = new();
    }

    public string GetOrCreate(string sessionId, string value, DetectorType type, int ttlSeconds = 3600)
    {
        Cleanup();
        var session = _sessions.GetOrAdd(sessionId, _ => new Session
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(ttlSeconds)
        });

        return session.ValueToToken.GetOrAdd(value, v =>
        {
            var counter = session.Counters.AddOrUpdate(type, 1, (_, c) => c + 1);
            var token = $"[{type.ToString().ToUpperInvariant()}_{counter}]";
            session.TokenToValue[token] = v;
            return token;
        });
    }

    /// <summary>Восстанавливает все токены в тексте для заданной сессии.</summary>
    public string RestoreAll(string sessionId, string text)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return text;

        foreach (var (token, original) in session.TokenToValue)
            text = text.Replace(token, original, StringComparison.Ordinal);

        return text;
    }

    private void Cleanup()
    {
        foreach (var key in _sessions.Where(kv => kv.Value.ExpiresAt < DateTime.UtcNow)
                                     .Select(kv => kv.Key).ToList())
            _sessions.TryRemove(key, out _);
    }
}
