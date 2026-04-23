using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Sanitizer.Service.Detectors;

public class GuidDetector
{
    private readonly Regex guidRegex;
    private readonly Regex guidNoDashRegex;
    private readonly Regex guidBraceRegex;

    public GuidDetector()
    {
        const string guidPattern = @"
            (?<=^|\s|[.,:;!?()\[\]<>""']|\n|\r|\t)
            [0-9a-fA-F]{8}          # 8 hex цифр
            -                         # Дефис
            [0-9a-fA-F]{4}          # 4 hex цифры
            -                         # Дефис
            [0-9a-fA-F]{4}          # 4 hex цифры
            -                         # Дефис
            [0-9a-fA-F]{4}          # 4 hex цифры
            -                         # Дефис
            [0-9a-fA-F]{12}         # 12 hex цифр
            (?=\s|$|[.,:;!?()\[\]<>""']|\n|\r|\t)
        ";
        
        const string guidNoDashPattern = @"
            (?<=^|\s|[.,:;!?()\[\]<>""']|\n|\r|\t)
            [0-9a-fA-F]{32}         # 32 hex цифры подряд
            (?=\s|$|[.,:;!?()\[\]<>""']|\n|\r|\t)
        ";
        
        const string guidBracePattern = @"
            (?<=^|\s|[.,:;!?()\[\]<>""']|\n|\r|\t)
            \{                        # Открывающая фигурная скобка
            [0-9a-fA-F]{8}          # 8 hex цифр
            -                         # Дефис
            [0-9a-fA-F]{4}          # 4 hex цифры
            -                         # Дефис
            [0-9a-fA-F]{4}          # 4 hex цифры
            -                         # Дефис
            [0-9a-fA-F]{4}          # 4 hex цифры
            -                         # Дефис
            [0-9a-fA-F]{12}         # 12 hex цифр
            \}                        # Закрывающая фигурная скобка
            (?=\s|$|[.,:;!?()\[\]<>""']|\n|\r|\t)
        ";

        guidRegex = new Regex(guidPattern,
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);

        guidNoDashRegex = new Regex(guidNoDashPattern,
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);

        guidBraceRegex = new Regex(guidBracePattern,
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);
    }

    public IReadOnlyCollection<GuidMatch> FindGuids(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var matches = new List<GuidMatch>();

        // Поиск UUID в фигурных скобках
        foreach (Match match in guidBraceRegex.Matches(text))
        {
            var guid = match.Value;
            var cleanGuid = guid.Trim('{', '}');
            
            if (IsValidGuid(cleanGuid))
            {
                matches.Add(new GuidMatch
                {
                    Guid = guid,
                    Position = match.Index
                });
            }
        }

        // Поиск UUID с дефисами
        foreach (Match match in guidRegex.Matches(text))
        {
            var guid = match.Value;
            
            if (IsValidGuid(guid))
            {
                matches.Add(new GuidMatch
                {
                    Guid = guid,
                    Position = match.Index
                });
            }
        }

        // Поиск UUID без дефисов
        foreach (Match match in guidNoDashRegex.Matches(text))
        {
            var guid = match.Value;
            
            if (IsValidGuid(guid))
            {
                matches.Add(new GuidMatch
                {
                    Guid = guid,
                    Position = match.Index
                });
            }
        }

        // Удаляем дубликаты (UUID может быть найден несколькими паттернами)
        matches = matches
            .GroupBy(m => new { m.Position })
            .Select(g => g.First())
            .OrderBy(m => m.Position)
            .ToList();

        return new ReadOnlyCollection<GuidMatch>(matches);
    }

    private bool IsValidGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            return false;

        var hexOnly = guid.Replace("-", "");

        if (hexOnly.Length != 32)
            return false;

        return hexOnly.All(c =>
            char.IsDigit(c) ||
            (c >= 'a' && c <= 'f') ||
            (c >= 'A' && c <= 'F'));
    }
}

public record GuidMatch
{
    public required string Guid { get; init; }
    public required int Position { get; init; }
}