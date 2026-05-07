using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public interface IMessageStorage
{
    Task<Message> AddMessageAsync(string chatId, MessageRequest message);
}
