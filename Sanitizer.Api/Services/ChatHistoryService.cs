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

    public async Task<Message> AddMessageAsync(string chatId, MessageRequest message)
    {
        return await messageStorage.AddMessageAsync(chatId, message);
    }

    public Task<string> DeleteChatAsync(string chatId) =>
        chatStorage.DeleteChatAsync(chatId);
}
