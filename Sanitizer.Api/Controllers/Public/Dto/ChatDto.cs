namespace Sanitizer.Api.Controllers.Public.Dto;

/// <summary>Краткая информация о чате.</summary>
public record ChatInfoDto(
    string Id,
    string Name,
    string? ProfileId,
    DateTime CreatedAt);

/// <summary>Запрос на создание чата.</summary>
public record CreateChatDto(
    string Name,
    string? ProfileId);

/// <summary>Запрос на обновление чата (имя/профиль).</summary>
public record UpdateChatDto(
    string? Name,
    string? ProfileId);

/// <summary>Полная информация о чате с историей сообщений.</summary>
public record ChatDetailDto(
    string Id,
    string Name,
    string? ProfileId,
    DateTime CreatedAt,
    MessageDto[] Messages);
