using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Chat;
using Sanitizer.Api.Models.Message;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public class EfChatStorage(SanitizerDbContext db) : IChatStorage
{
    public async Task<List<ChatInfo>> GetAllAsync()
    {
        var entities = await db.Chats.AsNoTracking().ToListAsync();
        return entities.Select(e => new ChatInfo { Id = e.Id, Name = e.Name, CreatedAt = e.CreatedAt, ProfileId = e.ProfileId }).ToList();
    }

    public async Task<ChatSession> GetByIdAsync(string chatId)
    {
        var entity = await db.Chats
            .Include(c => c.Messages)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == chatId)
            ?? throw new KeyNotFoundException($"Chat with id {chatId} not found");

        return MapToModel(entity);
    }
    
    public async Task UpdateProfileIdAsync(string id, string profileId)
    {
        var entity = await db.Chats.FirstOrDefaultAsync(c => c.Id == id)
                     ?? throw new KeyNotFoundException($"Chat with id {id} not found");

        entity.ProfileId = profileId;
        await db.SaveChangesAsync();
    }
    
    public async Task<string?> GetProfileIdAsync(string chatId)
    {
        var chat = await db.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
        return chat?.ProfileId;
    }
    
    public async Task<ChatEntity> SaveChatAsync(string name)
    {
        var entity = new ChatEntity { Name = name };
        db.Chats.Add(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    public async Task<string> DeleteChatAsync(string chatId)
    {
        var chat = await db.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat is not null)
        {
            db.Chats.Remove(chat);
            await db.SaveChangesAsync();
        }
        
        return chatId;
    }

    internal static ChatSession MapToModel(ChatEntity entity) => new()
    {
        Id = entity.Id,
        SanitizationProfileId = entity.ProfileId,
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
            }).ToList()
    };
}
