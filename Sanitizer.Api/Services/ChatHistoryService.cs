using Sanitizer.Api.Auth;
using Sanitizer.Api.Controllers.Internal.Client.Requests;
using Sanitizer.Api.Models.Chat;
using Sanitizer.Api.Models.Message;
using Sanitizer.Api.Storage;

namespace Sanitizer.Api.Services;

public class ChatHistoryService(
    IChatStorage chatStorage,
    IMessageStorage messageStorage,
    ICurrentApiKeyContext apiKeyContext)
{
    public Task<List<ChatInfo>> GetAllAsync() =>
        chatStorage.GetAllAsync(apiKeyContext.ApiKeyId);

    public async Task<List<ChatInfo>> UpdateAsync(string id, UpdateChatRequest chatRequest)
    {
        if (chatRequest.ProfileId is not null)
        {
            await chatStorage.UpdateProfileIdAsync(id, chatRequest.ProfileId, apiKeyContext.ApiKeyId);
        }

        return await GetAllAsync();
    }

    public Task<ChatSession?> GetByIdAsync(string chatId) =>
        chatStorage.GetByIdAsync(chatId, apiKeyContext.ApiKeyId);

    public Task<string?> GetProfileIdAsync(string chatId) =>
        chatStorage.GetProfileIdAsync(chatId, apiKeyContext.ApiKeyId);

    public async Task<ChatInfo> SaveChatAsync(string name)
    {
        var chatEntity = await chatStorage.SaveChatAsync(name, apiKeyContext.ApiKeyId);
        return new ChatInfo
        {
            Id = chatEntity.Id,
            Name = chatEntity.Name,
            ProfileId = chatEntity.ProfileId,
            CreatedAt = chatEntity.CreatedAt
        };
    }

    public Task<Message> AddMessageAsync(string chatId, MessageRequest message) =>
        messageStorage.AddMessageAsync(chatId, message, apiKeyContext.ApiKeyId);

    public Task<bool> DeleteChatAsync(string chatId) =>
        chatStorage.DeleteChatAsync(chatId, apiKeyContext.ApiKeyId);
}
