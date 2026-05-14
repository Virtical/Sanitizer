using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Models;
using Sanitizer.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers;

/// <summary>CRUD для профилей санитизации.</summary>
[ApiController]
[Route("api/profiles")]
public class ProfilesController(ProfileService profileService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Получение существующий профилей")]
    public async Task<IActionResult> GetAll() =>
        Ok(await profileService.GetAllAsync());

    [SwaggerOperation(Summary = "Получение профиля")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var profile = await profileService.GetByIdAsync(id);
        return Ok(profile);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Создание профиля")]
    public async Task<IActionResult> Create([FromBody] ProfileCreateRequest request)
    {
        var profile = new SanitizationProfile
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Rules = request.Rules
        };

        await profileService.CreateAsync(profile);
        return await GetAll();
    }
    
    [SwaggerIgnore]
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Обновление профиля")]
    public async Task<IActionResult> Update(string id, [FromBody] SanitizationProfile profile)
    {
        var updated = await profileService.UpdateAsync(id, profile);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Удалить профиль.</summary>
    [SwaggerIgnore]
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Удаление профиля")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await profileService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
