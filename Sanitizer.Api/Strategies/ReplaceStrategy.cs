using Sanitizer.Api.Models;

namespace Sanitizer.Api.Strategies;

/// <summary>Заменяет значение статической строкой (по умолчанию [TYPE_REDACTED]).</summary>
public class ReplaceStrategy : ISanitizationStrategy
{
    public string Apply(string value, DetectorType type, string _) => $"[{type.ToString().ToUpperInvariant()}_REDACTED]";
}
