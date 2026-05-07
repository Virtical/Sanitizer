using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public interface IChatHistoryStorage
{
    /// <summary>Все чаты без сообщений (для списка диалогов).</summary>
    Task<List<ChatInfo>> GetAllAsync();

    /// <summary>Чат со всеми сообщениями.</summary>
    Task<ChatSession> GetByIdAsync(string chatId);

    /// <summary>Создать или обновить чат (без сообщений).</summary>
    Task<string> SaveChatAsync(string name);

    /// <summary>Добавить сообщение в чат</summary>
    Task<ChatSession> AddMessageAsync(string chatId, MessageRequest message);

    /// <summary>Удалить чат со всеми сообщениями.</summary>
    Task DeleteChatAsync(string chatId);
}
