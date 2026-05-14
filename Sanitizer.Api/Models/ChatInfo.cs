namespace Sanitizer.Api.Models;

public class ChatInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Новый диалог";
    public string? ProfileId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
