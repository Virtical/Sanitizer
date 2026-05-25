using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Controllers.Public.Dto;
using Sanitizer.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers.Public;

/// <summary>Управление профилями санитизации (публичный API).</summary>
[ApiController]
[Route("api/public/v1/profiles")]
[Tags("Public – Profiles")]
public class PublicProfilesController(ProfileService profileService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Получить список профилей токена")]
    [ProducesResponseType(typeof(ProfileDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var profiles = await profileService.GetAllAsync();
        var dtos = profiles
            .Select(p => new ProfileDto(p.Id, p.Name, p.Rules))
            .ToArray();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Получить профиль по id")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var profile = await profileService.GetByIdAsync(id);
        if (profile is null) return NotFound();
        return Ok(new ProfileDto(profile.Id, profile.Name, profile.Rules));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Создать профиль")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateProfileDto dto)
    {
        var request = new CreateProfileRequest(dto.Name, dto.Rules);
        await profileService.CreateAsync(request);

        var all = await profileService.GetAllAsync();
        var created = all.Last();
        var result = new ProfileDto(created.Id, created.Name, created.Rules);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Обновить профиль")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateProfileDto dto)
    {
        var request = new UpdateProfileRequest(dto.Name, dto.Rules);
        var updated = await profileService.UpdateAsync(id, request);
        if (updated is null) return NotFound();
        return Ok(new ProfileDto(updated.Id, updated.Name, updated.Rules));
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Удалить профиль")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var deleted = await profileService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
