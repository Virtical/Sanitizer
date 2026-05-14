using Sanitizer.Api.Models;

namespace Sanitizer.Api.Strategies;

/// <summary>Полностью удаляет найденное значение из текста.</summary>
public class RemoveStrategy : ISanitizationStrategy
{
    public string Apply(string value, DetectorType _, string __) => string.Empty;
}
