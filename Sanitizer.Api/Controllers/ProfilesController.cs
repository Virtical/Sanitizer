using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Controllers.Client.Requests;
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

    [HttpPost]
    [SwaggerOperation(Summary = "Создание профиля")]
    public async Task<IActionResult> Create([FromBody] CreateProfileRequest request)
    {
        await profileService.CreateAsync(request);
        return await GetAll();
    }
    
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Обновление профиля")]
    public async Task<IActionResult> Update([FromRoute]string id, [FromBody] UpdateProfileRequest profileRequest)
    {
        var updated = await profileService.UpdateAsync(id, profileRequest);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Удалить профиль.</summary>
    [SwaggerIgnore]
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Удаление профиля")]
    public async Task<IActionResult> Delete([FromRoute]string id)
    {
        var deleted = await profileService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
