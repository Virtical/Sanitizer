using Sanitizer.Api.Models;

namespace Sanitizer.Api.Strategies;

public interface ISanitizationStrategy
{
    string Apply(string value, DetectorType type, Dictionary<string, string> parameters, string sessionId);
}
