namespace Sanitizer.Api.Models.Strategy;

public class StrategyConfig
{
    public required StrategyType Strategy { get; init; }
    
    public static implicit operator StrategyConfig(StrategyType strategy) => new() { Strategy = strategy };
}
