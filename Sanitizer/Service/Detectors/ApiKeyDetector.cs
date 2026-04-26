using System.Text.RegularExpressions;

namespace Sanitizer.Service.Detectors;

public class ApiKeyDetector : IDetector
{
    /// <summary>Настраиваемый список паттернов для кастомных API.</summary>
    public static readonly List<Regex> Patterns =
    [
        // OpenAI: sk-XXXX (минимум 20 символов)
        new Regex(@"sk-[A-Za-z0-9]{20,}", RegexOptions.Compiled),

        // GitHub tokens: ghp_ (personal), gho_ (OAuth), ghs_ (server), ghr_ (refresh)
        new Regex(@"gh[porsh]_[A-Za-z0-9]{36,}", RegexOptions.Compiled),

        // Ключи с префиксами: api_key=, token=, secret=, access_token=, auth_token=, api-key=
        new Regex(
            @"(?:api[_-]?key|token|secret|access[_-]?token|auth[_-]?token)\s*[=:]\s*[\w\-\.]{8,}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase),
    ];

    public ItemMatch[] Find(string text)
    {
        return Patterns
            .SelectMany(r => r.Matches(text))
            .Select(m => new ItemMatch { Value = m.Value, Position = m.Index })
            .OrderBy(m => m.Position)
            .ToArray();
    }
}
