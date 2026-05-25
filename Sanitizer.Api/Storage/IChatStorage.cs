using Sanitizer.Api.Models.Chat;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public interface IChatStorage
{
    /// <summary>Получить список чатов, принадлежащих указанному API-ключу.</summary>
    Task<List<ChatInfo>> GetAllAsync(Guid apiKeyId);

    /// <summary>
    /// Получить чат с сообщениями.
    /// Возвращает null, если чат не найден или принадлежит другому ключу.
    /// </summary>
    Task<ChatSession?> GetByIdAsync(string chatId, Guid apiKeyId);

    /// <summary>Создать новый чат.</summary>
    Task<ChatEntity> SaveChatAsync(string name, Guid apiKeyId);

    /// <summary>Обновить ProfileId чата.</summary>
    Task UpdateProfileIdAsync(string id, string? profileId, Guid apiKeyId);

    /// <summary>Получить ProfileId чата. Возвращает null, если чат не найден или нет профиля.</summary>
    Task<string?> GetProfileIdAsync(string chatId, Guid apiKeyId);

    /// <summary>
    /// Удалить чат.
    /// Возвращает false, если чат не найден или принадлежит другому ключу.
    /// </summary>
    Task<bool> DeleteChatAsync(string chatId, Guid apiKeyId);
}
