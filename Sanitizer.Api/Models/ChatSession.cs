namespace Sanitizer.Api.Models;

public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? SanitizationProfileId { get; set; }

    public List<Message> Messages { get; set; } = new();
}
