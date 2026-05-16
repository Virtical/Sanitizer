using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Message;

namespace Sanitizer.Api.Storage;

public interface IMessageStorage
{
    Task<Message> AddMessageAsync(string chatId, MessageRequest message);
}
