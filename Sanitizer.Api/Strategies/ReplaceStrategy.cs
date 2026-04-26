using Sanitizer.Api.Models;

namespace Sanitizer.Api.Strategies;

/// <summary>Заменяет значение статической строкой (по умолчанию [TYPE_REDACTED]).</summary>
public class ReplaceStrategy : ISanitizationStrategy
{
    public string Apply(string value, DetectorType type, Dictionary<string, string> parameters, string _)
    {
        var label = type.ToString().ToUpperInvariant();
        return parameters.GetValueOrDefault("replacement", $"[{label}_REDACTED]");
    }
}
