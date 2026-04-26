namespace Sanitizer.Api.Data.Entities;

public class SanitizationProfileEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool Reversible { get; set; }

    public List<SanitizationRuleEntity> Rules { get; set; } = new();
}
