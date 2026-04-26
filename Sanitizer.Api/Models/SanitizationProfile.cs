namespace Sanitizer.Api.Models;

public class SanitizationProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Если true — поддерживается восстановление (стратегия Tokenize).
    /// </summary>
    public bool Reversible { get; set; }

    /// <summary>
    /// Правила: тип данных → конфигурация стратегии санитизации.
    /// </summary>
    public Dictionary<DetectorType, StrategyConfig> Rules { get; set; } = new();
}
