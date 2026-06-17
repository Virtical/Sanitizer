using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;

namespace Sanitizer.Api.Controllers.Internal.Client.Responses;

public record ProfileResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Dictionary<DetectorType, StrategyConfig> Rules { get; init; }

    public static ProfileResponse Default
        = new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = string.Empty,
            Rules = new Dictionary<DetectorType, StrategyConfig>()
        };
}