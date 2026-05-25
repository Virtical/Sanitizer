namespace Sanitizer.Api.Storage.Data.Entities;

public class ChatEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Новый диалог";
    public string? ProfileId { get; set; }

    /// <summary>
    /// Владелец чата.
    /// <see cref="Guid.Empty"/> означает системный чат (внутренний UI).
    /// </summary>
    public Guid ApiKeyId { get; set; } = Guid.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<MessageEntity> Messages { get; set; } = new();
}
