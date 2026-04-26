namespace Sanitizer.Api.Models;

public class ApiKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;

    /// <summary>SHA-256 хеш ключа в hex-формате.</summary>
    public string KeyHash { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}
