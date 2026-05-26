using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Models.Strategy;
using Sanitizer.Api.Services;
using Sanitizer.Api.Controllers.Public.Dto;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers.Public;

/// <summary>Метаинформация о доступных детекторах и стратегиях (публичный API).</summary>
[ApiController]
[Route("api/public/meta")]
[Tags("Public – Meta")]
public class PublicMetaController(DetectorRegistry detectorRegistry) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Получить список доступных детекторов и стратегий",
        Description = "Возвращает все зарегистрированные детекторы и поддерживаемые стратегии санитизации.")]
    [ProducesResponseType(typeof(MetaDto), StatusCodes.Status200OK)]
    public IActionResult GetMeta()
    {
        var allStrategies = Enum.GetNames<StrategyType>();

        var detectors = detectorRegistry.All.Keys
            .Select(detectorType => new DetectorMetaDto(
                detectorType.ToString(),
                allStrategies))
            .ToArray();

        return Ok(new MetaDto(detectors));
    }
}
