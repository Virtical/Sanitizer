namespace Sanitizer.Api.Models;

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ChatId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
    public MessageType Type { get; set; }

    /// <summary>Порядковый номер сообщения в чате (0-based).</summary>
    public int OrderIndex { get; set; }
}
