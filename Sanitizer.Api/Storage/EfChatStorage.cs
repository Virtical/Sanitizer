using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Models.Chat;
using Sanitizer.Api.Models.Message;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public class EfChatStorage(SanitizerDbContext db) : IChatStorage
{
    public async Task<List<ChatInfo>> GetAllAsync(Guid apiKeyId)
    {
        var entities = await db.Chats
            .AsNoTracking()
            .Where(c => c.ApiKeyId == apiKeyId)
            .ToListAsync();

        return entities
            .Select(e => new ChatInfo
            {
                Id = e.Id,
                Name = e.Name,
                CreatedAt = e.CreatedAt,
                ProfileId = e.ProfileId
            })
            .ToList();
    }

    public async Task<ChatSession?> GetByIdAsync(string chatId, Guid apiKeyId)
    {
        var entity = await db.Chats
            .Include(c => c.Messages)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == chatId && c.ApiKeyId == apiKeyId);

        return entity is null ? null : MapToModel(entity);
    }

    public async Task<ChatEntity> SaveChatAsync(string name, Guid apiKeyId)
    {
        var entity = new ChatEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            ApiKeyId = apiKeyId
        };

        db.Chats.Add(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    public async Task UpdateProfileIdAsync(string id, string? profileId, Guid apiKeyId)
    {
        var entity = await db.Chats
            .FirstOrDefaultAsync(c => c.Id == id && c.ApiKeyId == apiKeyId)
            ?? throw new KeyNotFoundException($"Чат с id {id} не найден.");

        entity.ProfileId = profileId;
        await db.SaveChangesAsync();
    }

    public async Task<string?> GetProfileIdAsync(string chatId, Guid apiKeyId)
    {
        var chat = await db.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == chatId && c.ApiKeyId == apiKeyId);

        return chat?.ProfileId;
    }

    public async Task<bool> DeleteChatAsync(string chatId, Guid apiKeyId)
    {
        var chat = await db.Chats
            .FirstOrDefaultAsync(c => c.Id == chatId && c.ApiKeyId == apiKeyId);

        if (chat is null) return false;

        db.Chats.Remove(chat);
        await db.SaveChangesAsync();
        return true;
    }

    internal static ChatSession MapToModel(ChatEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        ProfileId = entity.ProfileId,
        CreatedAt = entity.CreatedAt,
        Messages = entity.Messages
            .OrderBy(m => m.OrderIndex)
            .Select(m => new Message
            {
                Id = m.Id,
                ChatId = m.ChatId,
                Text = m.Text,
                Type = m.Type,
                OriginalMessageId = m.OriginalMessageId,
                OrderIndex = m.OrderIndex
            })
            .ToList()
    };
}
