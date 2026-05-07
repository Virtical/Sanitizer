namespace Sanitizer.Api.Models;

public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID профиля санитизации, выбранного для данного чата.
    /// Null — профиль не выбран.
    /// </summary>
    public string? SanitizationProfileId { get; set; }

    public List<Message> Messages { get; set; } = new();
}
