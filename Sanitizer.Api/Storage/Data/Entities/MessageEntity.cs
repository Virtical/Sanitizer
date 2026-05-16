using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Message;

namespace Sanitizer.Api.Storage.Data.Entities;

public class MessageEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string ChatId { get; set; } = string.Empty;
    public ChatEntity? Chat { get; set; }
    public string Text { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public int OrderIndex { get; set; }
    public string? OriginalMessageId { get; set; }
}
