namespace Sanitizer.Api.Models;

public class ApiKey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    /// <summary>SHA-256 хеш ключа в hex-формате.</summary>
    public string KeyHash { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public bool IsValid() => IsActive && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);
}
