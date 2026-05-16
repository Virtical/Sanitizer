namespace Sanitizer.Api.Models.Message;

public class Message
{
    public required string Id { get; init; }
    public required string ChatId { get; init; }
    public required string Text { get; init; }
    public required MessageType Type { get; init; }
    public required int OrderIndex { get; init; }
    public string? OriginalMessageId { get; init; }
}
