namespace Sanitizer.Api.Models;

public class MessageRequest
{
    public string Text { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public string? OriginalMessageId { get; set; }
}
