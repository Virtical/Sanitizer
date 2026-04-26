using Sanitizer.Api.Models;
using Sanitizer.Api.Strategies;
using Sanitizer.Service.Detectors;

namespace Sanitizer.Api.Services;

/// <summary>
/// Основной сервис санитизации.
/// Запускает детекторы по правилам профиля, разрешает пересечения,
/// применяет стратегии справа налево (не сбивая индексы).
/// </summary>
public class SanitizerService(DetectorRegistry registry, StrategyFactory strategyFactory)
{
    public SanitizationResult Sanitize(string text, SanitizationProfile profile)
    {
        var sessionId = Guid.NewGuid().ToString();

        // 1. Собираем все совпадения по активным правилам
        var allMatches = profile.Rules
            .SelectMany(rule =>
                registry.Get(rule.Key).Find(text)
                    .Select(m => (Match: m, Type: rule.Key, Config: rule.Value)))
            .ToList();

        // 2. Убираем пересечения: побеждает более длинное, при равенстве — более раннее
        var nonOverlapping = RemoveOverlaps(allMatches);

        // 3. Применяем стратегии справа налево
        var sanitizedText = text;
        var items = new List<SanitizedItem>();

        foreach (var (match, type, config) in nonOverlapping)
        {
            var strategy  = strategyFactory.Create(config.Strategy);
            var sanitized = strategy.Apply(match.Value, type, config.Parameters, sessionId);

            items.Add(new SanitizedItem
            {
                OriginalValue  = match.Value,
                SanitizedValue = sanitized,
                Type           = type,
                Position       = match.Position,
                Strategy       = config.Strategy
            });

            sanitizedText = sanitizedText
                .Remove(match.Position, match.Value.Length)
                .Insert(match.Position, sanitized);
        }

        return new SanitizationResult
        {
            OriginalText  = text,
            SanitizedText = sanitizedText,
            SessionId     = profile.Reversible ? sessionId : null,
            Items         = items.OrderBy(x => x.Position).ToList()
        };
    }

    private static List<(ItemMatch Match, DetectorType Type, StrategyConfig Config)> RemoveOverlaps(
        List<(ItemMatch Match, DetectorType Type, StrategyConfig Config)> matches)
    {
        var selected  = new List<(ItemMatch Match, DetectorType Type, StrategyConfig Config)>();
        var usedRanges = new List<(int Start, int End)>();

        foreach (var item in matches.OrderByDescending(x => x.Match.Value.Length)
                                    .ThenBy(x => x.Match.Position))
        {
            var start = item.Match.Position;
            var end   = start + item.Match.Value.Length;

            if (!usedRanges.Any(r => start < r.End && end > r.Start))
            {
                selected.Add(item);
                usedRanges.Add((start, end));
            }
        }

        // Возвращаем в порядке убывания позиции для замены справа налево
        return selected.OrderByDescending(x => x.Match.Position).ToList();
    }
}
