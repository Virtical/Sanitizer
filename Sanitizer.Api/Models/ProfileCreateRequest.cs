namespace Sanitizer.Api.Models;

public class ProfileCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<DetectorType, StrategyConfig> Rules { get; set; } = new();
}