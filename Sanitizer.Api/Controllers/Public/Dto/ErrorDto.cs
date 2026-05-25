namespace Sanitizer.Api.Controllers.Public.Dto;

/// <summary>
/// Унифицированный формат ошибок публичного API.
/// </summary>
/// <param name="Code">Машиночитаемый код ошибки (UPPER_SNAKE_CASE), например, <c>CHAT_NOT_FOUND</c>.</param>
/// <param name="Message">Человекочитаемое описание ошибки для клиента.</param>
public record ErrorDto(string Code, string Message);
