using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;

namespace Sanitizer.Api.Controllers.Public.Dto;

/// <summary>Профиль санитизации (публичный API).</summary>
public record ProfileDto(
    string Id,
    string Name,
    Dictionary<DetectorType, StrategyConfig> Rules);

/// <summary>Запрос на создание профиля санитизации.</summary>
public record CreateProfileDto(
    string Name,
    Dictionary<DetectorType, StrategyConfig> Rules);

/// <summary>Запрос на обновление профиля санитизации.</summary>
public record UpdateProfileDto(
    string Name,
    Dictionary<DetectorType, StrategyConfig> Rules);
