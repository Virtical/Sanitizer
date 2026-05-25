using Sanitizer.Api.Models.Message;

namespace Sanitizer.Api.Storage;

public interface IMessageStorage
{
    /// <summary>
    /// Добавить сообщение в чат.
    /// Проверяет принадлежность чата указанному API-ключу.
    /// </summary>
    Task<Message> AddMessageAsync(string chatId, MessageRequest message, Guid apiKeyId);
}
