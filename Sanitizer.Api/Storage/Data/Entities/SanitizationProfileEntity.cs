namespace Sanitizer.Api.Storage.Data.Entities;

public class SanitizationProfileEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Владелец профиля.
    /// <see cref="Guid.Empty"/> означает системный профиль (внутренний UI).
    /// </summary>
    public Guid ApiKeyId { get; set; } = Guid.Empty;

    public List<SanitizationRuleEntity> Rules { get; set; } = new();
}
