namespace Sanitizer.Api.Models.Chat;

public class ChatInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? ProfileId { get; init; }
    public required DateTime CreatedAt { get; init; }
}
