namespace Sanitizer.Api.Data.Entities;

public class ApiKeyEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
}
