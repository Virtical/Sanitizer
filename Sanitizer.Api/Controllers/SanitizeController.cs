using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Models;
using Sanitizer.Api.Services;

namespace Sanitizer.Api.Controllers;

[ApiController]
[Route("api/sanitize")]
public class SanitizeController(SanitizerService sanitizerService,
                                 ProfileService profileService,
                                 TokenStore tokenStore) : ControllerBase
{
    public record SanitizeRequest(string Text, string ProfileId);
    public record RestoreRequest(string Text, string SessionId);

    /// <summary>Санитизировать текст по профилю.</summary>
    [HttpPost]
    public async Task<IActionResult> Sanitize([FromBody] SanitizeRequest request)
    {
        var profile = await profileService.GetByIdAsync(request.ProfileId);
        if (profile is null) return NotFound($"Profile '{request.ProfileId}' not found.");

        var result = sanitizerService.Sanitize(request.Text, profile);
        return Ok(result);
    }

    /// <summary>
    /// Восстановить оригинальные значения из токенов.
    /// Работает только для профилей с Reversible=true (стратегия Tokenize).
    /// </summary>
    [HttpPost("restore")]
    public IActionResult Restore([FromBody] RestoreRequest request)
    {
        var restored = tokenStore.RestoreAll(request.SessionId, request.Text);
        return Ok(new { RestoredText = restored });
    }
}
