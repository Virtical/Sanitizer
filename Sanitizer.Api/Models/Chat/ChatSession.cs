namespace Sanitizer.Api.Models.Chat;

public class ChatSession
{
    public required string Id { get; init; }
    public string? SanitizationProfileId { get; init; }
    public required List<Message.Message> Messages { get; init; }
}
