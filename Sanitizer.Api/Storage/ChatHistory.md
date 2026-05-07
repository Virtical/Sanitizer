# ChatHistory — структура и идея реализации

## Модели (Models)

### MessageType (enum)
```csharp
public enum MessageType
{
    Sent,       // Отправлено пользователем
    Answer,     // Ответ системы/LLM
    Sanitized   // Санитизированная копия сообщения
}
```

### Message
```csharp
public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ChatId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
    public MessageType Type { get; set; }

    /// <summary>Порядковый номер сообщения в чате (0-based).</summary>
    public int OrderIndex { get; set; }
}
```

### MessageRequest
```csharp
public class MessageRequest
{
    public string Text { get; set; } = string.Empty;
    public MessageType Type { get; set; }
}
```

### ChatSession
```csharp
public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID профиля санитизации, выбранного для данного чата.
    /// Null — профиль не выбран.
    /// </summary>
    public string? SanitizationProfileId { get; set; }

    public List<Message> Messages { get; set; } = new();
}
```

### ChatInfoResponse
```csharp
public class ChatInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Название чата (обычно — первое сообщение пользователя, до 30 символов).</summary>
    public string Name { get; set; } = "Новый диалог";
}
```

---

## Сущности EF Core (Storage/Data/Entities)

### ChatEntity
| Поле                  | Тип       | Описание                              |
|-----------------------|-----------|---------------------------------------|
| Id                    | string PK | Guid                                  |
| Name                  | string    | Название чата                         |
| SanitizationProfileId | string?   | FK → Profiles.Id (no cascade)| |
| Messages              | навигация | List\<ChatMessageEntity\>             |

### MessageEntity
| Поле              | Тип       | Описание                                    |
|-------------------|-----------|---------------------------------------------|
| Id                | string PK | Guid                                        |
| ChatId            | string FK | → ChatSessions.Id (Cascade Delete)          |
| Text              | string    | Текст сообщения                             |
| Type              | int       | Enum MessageType                            |
| OrderIndex        | int       | Порядковый номер (индекс по (ChatId, Order))|

---

## Интерфейс IChatHistoryStorage

```csharp
public interface IChatHistoryStorage
{
    /// <summary>Все чаты без сообщений (для списка диалогов).</summary>
    Task<List<ChatInfoResponse>> GetAllAsync();

    /// <summary>Чат со всеми сообщениями.</summary>
    Task<ChatSession> GetByIdAsync(string chatId);

    /// <summary>Создать или обновить чат (без сообщений).</summary>
    Task<string> SaveChatAsync(string name);

    /// <summary>Добавить сообщение в чат</summary>
    Task<ChatSession> AddMessageAsync(string chatId, MessageRequest message);

    /// <summary>Удалить чат со всеми сообщениями.</summary>
    Task DeleteChatAsync(string chatId);
}
```

---

## Реализация EfChatHistoryStorage

Класс `EfChatHistoryStorage(SanitizerDbContext db)` реализует `IChatHistoryStorage`.

### Ключевые моменты

- **GetAllAsync** — выборка без `.Include(Messages)`, только заголовки чатов.
- **GetByIdAsync** — `.Include(c => c.Messages)` с сортировкой `OrderBy(m => m.OrderIndex)`.
- **AddMessageAsync** — вычисляет `OrderIndex = currentMaxIndex + 1`, сохраняет сообщение.
- **DeleteChatAsync** — удаление через `Cascade` на уровне БД, дополнительных запросов не требует.
- **SanitizationProfileId** — хранится как `string?`, внешний ключ без навигационного свойства и без `OnDelete(Cascade)` (профиль удаляется независимо от чатов).

### Конфигурация в OnModelCreating

```csharp
modelBuilder.Entity<ChatEntity>()
    .HasMany(c => c.Messages)
    .WithOne(m => m.Chat)
    .HasForeignKey(m => m.ChatId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<MessageEntity>()
    .HasIndex(m => new { m.ChatId, m.OrderIndex })
    .IsUnique();
```

---

## Регистрация в DI

```csharp
builder.Services.AddScoped<IChatHistoryStorage, EfChatHistoryStorage>();
```

Миграция добавляется командой:
```
dotnet ef migrations add AddChatHistory --project Sanitizer.Api
```
