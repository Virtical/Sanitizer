using Sanitizer.Api.Models.Strategy;

namespace Sanitizer.Api.Models;

public record CreateProfileRequest(string Name, Dictionary<DetectorType, StrategyConfig> Rules);