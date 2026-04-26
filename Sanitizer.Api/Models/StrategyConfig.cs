namespace Sanitizer.Api.Models;

public class StrategyConfig
{
    public StrategyType Strategy { get; set; }

    /// <summary>
    /// Дополнительные параметры стратегии:
    /// Mask:     maskChar (default "*"), visibleStart, visibleEnd
    /// Hash:     algorithm ("sha256"/"sha512"), length (default 16), encoding ("hex"/"base64")
    /// Tokenize: ttlSeconds (default 3600)
    /// Replace:  replacement (default "[TYPE_REDACTED]")
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}
