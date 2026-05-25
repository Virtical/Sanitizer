using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Models.Message;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public class EfMessageStorage(SanitizerDbContext db) : IMessageStorage
{
    public async Task<Message> AddMessageAsync(string chatId, MessageRequest message, Guid apiKeyId)
    {
        var chat = await db.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId && c.ApiKeyId == apiKeyId)
            ?? throw new KeyNotFoundException($"Чат с id {chatId} не найден.");

        var maxIndex = chat.Messages.Any()
            ? chat.Messages.Max(m => m.OrderIndex)
            : -1;

        var entity = new MessageEntity
        {
            Id = Guid.NewGuid().ToString(),
            ChatId = chatId,
            Text = message.Text,
            Type = message.Type,
            OriginalMessageId = message.OriginalMessageId,
            OrderIndex = maxIndex + 1
        };

        chat.Messages.Add(entity);
        await db.SaveChangesAsync();

        return new Message
        {
            Id = entity.Id,
            ChatId = entity.ChatId,
            Text = entity.Text,
            Type = entity.Type,
            OriginalMessageId = entity.OriginalMessageId,
            OrderIndex = entity.OrderIndex
        };
    }
}
