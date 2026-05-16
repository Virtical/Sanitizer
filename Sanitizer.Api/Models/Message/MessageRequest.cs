namespace Sanitizer.Api.Models.Message;

public class MessageRequest
{
    public required string Text { get; init; }
    public required MessageType Type { get; init; }
    public string? OriginalMessageId { get; init; }
    
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
