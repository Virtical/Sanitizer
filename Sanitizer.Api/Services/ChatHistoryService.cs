using Sanitizer.Api.Models;
using Sanitizer.Api.Storage;

namespace Sanitizer.Api.Services;

public class ChatHistoryService(IChatStorage chatStorage, IMessageStorage messageStorage) : IChatHistoryStorage
{
    public Task<List<ChatInfo>> GetAllAsync() =>
        chatStorage.GetAllAsync();

    public Task<ChatSession> GetByIdAsync(string chatId) =>
        chatStorage.GetByIdAsync(chatId);

    public Task<string> SaveChatAsync(string name) =>
        chatStorage.SaveChatAsync(name);

    public async Task<ChatSession> AddMessageAsync(string chatId, MessageRequest message)
    {
        await messageStorage.AddMessageAsync(chatId, message);
        return await chatStorage.GetByIdAsync(chatId);
    }

    public Task DeleteChatAsync(string chatId) =>
        chatStorage.DeleteChatAsync(chatId);
}
