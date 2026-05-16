using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Message;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public class EfMessageStorage(SanitizerDbContext db) : IMessageStorage
{
    public async Task<Message> AddMessageAsync(string chatId, MessageRequest message)
    {
        var chat = await db.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId)
            ?? throw new KeyNotFoundException($"Chat with id {chatId} not found");

        var maxIndex = chat.Messages.Any()
            ? chat.Messages.Max(m => m.OrderIndex)
            : -1;

        var entity = new MessageEntity
        {
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
