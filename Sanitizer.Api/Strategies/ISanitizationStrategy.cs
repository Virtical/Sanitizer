using Sanitizer.Api.Models;

namespace Sanitizer.Api.Strategies;

public interface ISanitizationStrategy
{
    string Apply(string value, DetectorType type, string sessionId);
}
