using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public interface IChatStorage
{
    Task<List<ChatInfo>> GetAllAsync();
    Task<ChatSession> GetByIdAsync(string chatId);
    Task<List<ChatInfo>> SaveChatAsync(string name);
    Task<string> DeleteChatAsync(string chatId);
    Task<string?> GetProfileIdAsync(string chatId);
}
