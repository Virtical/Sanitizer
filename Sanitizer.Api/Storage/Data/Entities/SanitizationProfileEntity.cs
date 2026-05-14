namespace Sanitizer.Api.Storage.Data.Entities;

public class SanitizationProfileEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;

    public List<SanitizationRuleEntity> Rules { get; set; } = new();
}
