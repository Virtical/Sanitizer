using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;

namespace Sanitizer.Api.Controllers.Public.Dto;

/// <summary>Запрос на отправку сообщения в чат.</summary>
public record SendMessageDto(string Message, Dictionary<DetectorType, StrategyConfig> Rules);

/// <summary>Ответ на синхронную отправку сообщения.</summary>
public record MessageResponseDto(string Sanitize, string Answer);
