using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public interface IProfileStorage
{
    /// <summary>Получить все профили, принадлежащие указанному API-ключу.</summary>
    Task<SanitizationProfileEntity[]> GetAllAsync(Guid apiKeyId);

    /// <summary>
    /// Получить профиль по id.
    /// Возвращает null, если профиль не найден или принадлежит другому ключу.
    /// </summary>
    Task<SanitizationProfileEntity?> GetByIdAsync(string id, Guid apiKeyId);
    
    /// <summary>
    /// Получить профиль по имени.
    /// Возвращает null, если профиль не найден или принадлежит другому ключу.
    /// </summary>
    Task<SanitizationProfileEntity?> GetByNameAsync(string name, Guid apiKeyId);

    /// <summary>Сохранить (создать или обновить) профиль.</summary>
    Task SaveAsync(SanitizationProfileEntity profile);

    /// <summary>
    /// Удалить профиль.
    /// Возвращает false, если профиль не найден или принадлежит другому ключу.
    /// </summary>
    Task<bool> DeleteAsync(string id, Guid apiKeyId);
}
