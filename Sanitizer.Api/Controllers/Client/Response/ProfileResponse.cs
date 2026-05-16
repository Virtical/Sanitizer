using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;

namespace Sanitizer.Api.Controllers.Client.Response;

public record ProfileResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Dictionary<DetectorType, StrategyConfig> Rules { get; init; }

    public static ProfileResponse Default
        = new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = String.Empty,
            Rules = new Dictionary<DetectorType, StrategyConfig>()
        };
}