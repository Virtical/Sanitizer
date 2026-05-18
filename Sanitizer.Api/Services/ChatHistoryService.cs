using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Chat;
using Sanitizer.Api.Models.Message;
using Sanitizer.Api.Storage;

namespace Sanitizer.Api.Services;

public class ChatHistoryService(IChatStorage chatStorage, IMessageStorage messageStorage)
{
    public Task<List<ChatInfo>> GetAllAsync() =>
        chatStorage.GetAllAsync();

    public async Task<List<ChatInfo>> UpdateAsync(string id, UpdateChatRequest chatRequest)
    {
        if (chatRequest.ProfileId is not null)
        {
            await chatStorage.UpdateProfileIdAsync(id, chatRequest.ProfileId);
        }

        return await GetAllAsync();
    }

    public Task<ChatSession> GetByIdAsync(string chatId) =>
        chatStorage.GetByIdAsync(chatId);

    public Task<string?> GetProfileIdAsync(string chatId) =>
        chatStorage.GetProfileIdAsync(chatId);

    public async Task<ChatInfo> SaveChatAsync(string name)
    {
        var chatEntity = await chatStorage.SaveChatAsync(name);
        return new ChatInfo
        {
            Id = chatEntity.Id,
            Name = chatEntity.Name,
            ProfileId = chatEntity.ProfileId,
            CreatedAt = chatEntity.CreatedAt
        };
    }

    public async Task<Message> AddMessageAsync(string chatId, MessageRequest message)
    {
        return await messageStorage.AddMessageAsync(chatId, message);
    }

    public Task<string> DeleteChatAsync(string chatId) =>
        chatStorage.DeleteChatAsync(chatId);
}
