using Sanitizer.Api.Models;
using Sanitizer.Api.Storage;

namespace Sanitizer.Api.Services;

public class ChatHistoryService(IChatStorage chatStorage, IMessageStorage messageStorage)
{
    public Task<List<ChatInfo>> GetAllAsync() =>
        chatStorage.GetAllAsync();

    public Task<ChatSession> GetByIdAsync(string chatId) =>
        chatStorage.GetByIdAsync(chatId);

    public Task<string?> GetProfileIdAsync(string chatId) =>
        chatStorage.GetProfileIdAsync(chatId);

    public Task<List<ChatInfo>> SaveChatAsync(string name) =>
        chatStorage.SaveChatAsync(name);

    public async Task<ChatSession> AddMessageAsync(string chatId, MessageRequest message)
    {
        await messageStorage.AddMessageAsync(chatId, message);
        return await chatStorage.GetByIdAsync(chatId);
    }

    public Task<string> DeleteChatAsync(string chatId) =>
        chatStorage.DeleteChatAsync(chatId);
}
