namespace Sanitizer.Api.Controllers.Public.Dto;

/// <summary>Сообщение в истории чата.</summary>
public record MessageDto(
    string Id,
    string Kind,
    string Text,
    string? ParentId,
    DateTime CreatedAt);

/// <summary>Запрос на отправку сообщения в чат.</summary>
public record SendMessageDto(string Message);

/// <summary>Ответ на синхронную отправку сообщения.</summary>
public record MessageResponseDto(
    string MessageId,
    string Answer);
