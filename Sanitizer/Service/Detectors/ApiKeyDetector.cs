using System.Text.RegularExpressions;

namespace Sanitizer.Service.Detectors;

public class ApiKeyDetector
{
    private readonly List<Regex> patterns;

    public ApiKeyDetector(IEnumerable<string>? customPatterns = null)
    {
        patterns = new List<Regex>
        {
            // OpenAI: sk-XXXXXXXXXXXXXXXXXXXX
            new Regex(@"\bsk-[A-Za-z0-9]{20,}\b",
                RegexOptions.Compiled | RegexOptions.CultureInvariant),

            // GitHub: ghp_XXXX, gho_XXXX
            new Regex(@"\bgh[pousr]_[A-Za-z0-9]{20,}\b",
                RegexOptions.Compiled | RegexOptions.CultureInvariant),

            // Общие ключи с префиксами
            new Regex(@"\b(api_key|token|secret)\s*=\s*[A-Za-z0-9\-_]{16,}\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        };

        // Кастомные паттерны
        if (customPatterns != null)
        {
            patterns.AddRange(customPatterns.Select(p =>
                new Regex(p, RegexOptions.Compiled | RegexOptions.CultureInvariant)));
        }
    }

    public IReadOnlyCollection<ApiKeyMatch> FindKeys(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var matches = new List<ApiKeyMatch>();

        foreach (var regex in patterns)
        {
            foreach (Match match in regex.Matches(text))
            {
                matches.Add(new ApiKeyMatch
                {
                    Value = match.Value,
                    Position = match.Index
                });
            }
        }

        // удаляем дубликаты по позиции
        return matches
            .GroupBy(m => m.Position)
            .Select(g => g.First())
            .OrderBy(m => m.Position)
            .ToList()
            .AsReadOnly();
    }
}

public record ApiKeyMatch
{
    public required string Value { get; init; }
    public required int Position { get; init; }
}