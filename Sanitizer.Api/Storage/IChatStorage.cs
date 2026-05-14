using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public interface IChatStorage
{
    Task<List<ChatInfo>> GetAllAsync();
    Task<ChatSession> GetByIdAsync(string chatId);
    Task SaveChatAsync(string name);
    Task UpdateNameAsync(string id, string name);
    Task UpdateProfileIdAsync(string id, string name);
    Task<string> DeleteChatAsync(string chatId);
    Task<string?> GetProfileIdAsync(string chatId);
}
