using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Models;
using Sanitizer.Api.Services;

namespace Sanitizer.Api.Controllers;

/// <summary>CRUD для профилей санитизации.</summary>
[ApiController]
[Route("api/profiles")]
public class ProfilesController(ProfileService profileService) : ControllerBase
{
    /// <summary>Список всех профилей.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await profileService.GetAllAsync());

    /// <summary>Получить профиль по ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var profile = await profileService.GetByIdAsync(id);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Создать новый профиль.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SanitizationProfile profile)
    {
        var created = await profileService.CreateAsync(profile);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Обновить существующий профиль.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] SanitizationProfile profile)
    {
        var updated = await profileService.UpdateAsync(id, profile);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Удалить профиль.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await profileService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
