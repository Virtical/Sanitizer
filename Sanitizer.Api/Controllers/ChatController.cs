using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Models;
using Sanitizer.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers;

/// <summary>
/// LLM Proxy: санитизирует входящие сообщения по профилю,
/// проксирует запрос к LLM, возвращает ответ вместе со списком замен.
/// </summary>
[ApiController]
[Route("api/chat")]
public class ChatController(SanitizerService sanitizerService,
                             ProfileService profileService,
                             ILlmClient llmClient,
                             DesanitizerService desanitizer) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ChatSendRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileId))
        {
            return BadRequest("ProfileId is required.");
        }

        if (string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("Message is required.");
        }

        var profile = await profileService.GetByIdAsync(request.ProfileId);
        if (profile is null)
        {
            return NotFound($"Profile '{request.ProfileId}' not found.");
        }

        SanitizationResult sanitization;
        try
        {
            sanitization = sanitizerService.Sanitize(request.Message, profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }

        string raw;
        try
        {
            raw = await llmClient.GetCompletionAsync(sanitization.SanitizedText);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { Error = "LLM provider error.", Details = ex.Message });
        }

        var final = desanitizer.Desanitize(raw, sanitization.Context);

        return Ok(new ChatSendResponse(sanitization.SanitizedText, final));
    }
}
