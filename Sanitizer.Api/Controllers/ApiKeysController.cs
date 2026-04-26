using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Services;

namespace Sanitizer.Api.Controllers;

/// <summary>
/// Управление API-ключами (только для AdminKey).
/// </summary>
[ApiController]
[Route("api/apikeys")]
public class ApiKeysController(ApiKeyService apiKeyService) : ControllerBase
{
    public record CreateKeyRequest(string Name, DateTime? ExpiresAt);

    /// <summary>Список ключей (без открытых значений).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsAdmin()) return Forbid();
        var keys = await apiKeyService.GetAllAsync();
        return Ok(keys.Select(k => new { k.Id, k.Name, k.CreatedDate, k.ExpiresAt, k.IsActive }));
    }

    /// <summary>
    /// Создать новый API-ключ.
    /// Открытый ключ возвращается единожды — сохраните его.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateKeyRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var (model, plain) = await apiKeyService.CreateAsync(request.Name, request.ExpiresAt);
        return Ok(new
        {
            model.Id, model.Name, model.CreatedDate, model.ExpiresAt,
            ApiKey  = plain,
            Warning = "Store this key safely — it will not be shown again."
        });
    }

    /// <summary>Деактивировать ключ.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id)
    {
        if (!IsAdmin()) return Forbid();
        var ok = await apiKeyService.DeactivateAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private bool IsAdmin() =>
        HttpContext.Items.TryGetValue("IsAdmin", out var v) && v is true;
}
