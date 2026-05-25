using System.ComponentModel.DataAnnotations;

namespace Sanitizer.Api.Storage.Data.Entities;

/// <summary>EF-сущность API-ключа.</summary>
public sealed class ApiKeyEntity
{
    /// <summary>Сам ключ в формате GUID — используется как Id и как токен доступа.</summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>SHA-256 хеш ключа в hex-формате.</summary>
    [Required, MaxLength(64)]
    public string KeyHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }
}
