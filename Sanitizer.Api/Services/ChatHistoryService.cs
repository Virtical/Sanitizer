using Sanitizer.Api.Models;
using Sanitizer.Api.Storage;

namespace Sanitizer.Api.Services;

public class ChatHistoryService(IChatStorage chatStorage, IMessageStorage messageStorage)
{
    public Task<List<ChatInfo>> GetAllAsync() =>
        chatStorage.GetAllAsync();

    public async Task<List<ChatInfo>> UpdateAsync(string id, UpdateRequest request)
    {
        if (request.Name is not null)
        {
            await chatStorage.UpdateNameAsync(id, request.Name);
        }
        
        if (request.ProfileId is not null)
        {
            await chatStorage.UpdateProfileIdAsync(id, request.ProfileId);
        }

        return await GetAllAsync();
    }

    public Task<ChatSession> GetByIdAsync(string chatId) =>
        chatStorage.GetByIdAsync(chatId);

    public Task<string?> GetProfileIdAsync(string chatId) =>
        chatStorage.GetProfileIdAsync(chatId);

    public async Task<List<ChatInfo>> SaveChatAsync(string name)
    {
        await chatStorage.SaveChatAsync(name);
        return await GetAllAsync();
    }

    public async Task<Message> AddMessageAsync(string chatId, MessageRequest message)
    {
        return await messageStorage.AddMessageAsync(chatId, message);
    }

    public Task<string> DeleteChatAsync(string chatId) =>
        chatStorage.DeleteChatAsync(chatId);
}
