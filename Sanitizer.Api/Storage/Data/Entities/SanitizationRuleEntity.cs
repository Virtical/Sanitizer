using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;

namespace Sanitizer.Api.Storage.Data.Entities;

public class SanitizationRuleEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ProfileId { get; set; } = string.Empty;
    public SanitizationProfileEntity? Profile { get; set; }

    public DetectorType DetectorType { get; set; }
    public StrategyType StrategyType { get; set; }
}
