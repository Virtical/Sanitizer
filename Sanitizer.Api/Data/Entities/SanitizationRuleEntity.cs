using Sanitizer.Api.Models;

namespace Sanitizer.Api.Data.Entities;

public class SanitizationRuleEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ProfileId { get; set; } = string.Empty;
    public SanitizationProfileEntity? Profile { get; set; }

    public DetectorType DetectorType { get; set; }
    public StrategyType StrategyType { get; set; }

    /// <summary>Параметры стратегии в JSON.</summary>
    public string ParametersJson { get; set; } = "{}";
}
