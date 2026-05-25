namespace Sanitizer.Api.Controllers.Public.Dto;

/// <summary>Описание одного детектора с поддерживаемыми стратегиями.</summary>
public record DetectorMetaDto(string Type, string[] Strategies);

/// <summary>Метаинформация о возможностях API.</summary>
public record MetaDto(DetectorMetaDto[] Detectors, string[] Strategies);
