using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Chat;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public interface IChatStorage
{
    Task<List<ChatInfo>> GetAllAsync();
    Task<ChatSession> GetByIdAsync(string chatId);
    Task<ChatEntity> SaveChatAsync(string name);
    Task UpdateNameAsync(string id, string name);
    Task UpdateProfileIdAsync(string id, string name);
    Task<string> DeleteChatAsync(string chatId);
    Task<string?> GetProfileIdAsync(string chatId);
}
