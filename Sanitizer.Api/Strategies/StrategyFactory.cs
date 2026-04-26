using Sanitizer.Api.Models;
using Sanitizer.Api.Services;

namespace Sanitizer.Api.Strategies;

public class StrategyFactory(TokenStore tokenStore)
{
    public ISanitizationStrategy Create(StrategyType type) => type switch
    {
        StrategyType.Mask     => new MaskStrategy(),
        StrategyType.Hash     => new HashStrategy(),
        StrategyType.Tokenize => new TokenizeStrategy(tokenStore),
        StrategyType.Replace  => new ReplaceStrategy(),
        StrategyType.Remove   => new RemoveStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
