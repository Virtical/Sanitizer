using System.ComponentModel.DataAnnotations;

namespace Sanitizer.Api.Storage.Data.Entities;

public sealed class UserEntity
{
    [Key, MaxLength(20)]
    public required string Login { get; set; }
    
    [Required, MaxLength(64)]
    public required string Password { get; set; }

    public required Guid ApiKeyId { get; set; }
}
