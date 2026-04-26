namespace Sanitizer.Api.Models;

public class SanitizationResult
{
    public string OriginalText { get; set; } = string.Empty;
    public string SanitizedText { get; set; } = string.Empty;

    /// <summary>
    /// Заполняется только при стратегии Tokenize (для последующего восстановления).
    /// </summary>
    public string? SessionId { get; set; }

    public List<SanitizedItem> Items { get; set; } = new();
}

public class SanitizedItem
{
    public string OriginalValue { get; set; } = string.Empty;
    public string SanitizedValue { get; set; } = string.Empty;
    public DetectorType Type { get; set; }
    public int Position { get; set; }
    public StrategyType Strategy { get; set; }
}
