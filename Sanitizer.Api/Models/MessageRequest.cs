namespace Sanitizer.Api.Models;

public class MessageRequest
{
    public string Text { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public string? OriginalMessageId { get; set; }

    public static MessageRequest CreateSent(string text)
        => new() 
        {
            Text = text,
            Type = MessageType.Sent,
        };
    
    public static MessageRequest CreateAnswer(string text)
        => new() 
        {
            Text = text,
            Type = MessageType.Answer,
        };
    
    public static MessageRequest CreateSanitized(string text, string originalMessageId)
        => new() 
        {
            Text = text,
            OriginalMessageId = originalMessageId,
            Type = MessageType.Sanitized,
        };
}
