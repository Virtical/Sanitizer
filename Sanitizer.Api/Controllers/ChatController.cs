using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Models;
using Sanitizer.Api.Services;

namespace Sanitizer.Api.Controllers;

/// <summary>
/// LLM Proxy: санитизирует входящие сообщения по профилю,
/// проксирует запрос к LLM, возвращает ответ вместе со списком замен.
/// </summary>
[ApiController]
[Route("api/chat")]
public class ChatController(SanitizerService sanitizerService,
                             ProfileService profileService,
                             LlmProxyService llmProxy) : ControllerBase
{
    [HttpPost("completions")]
    public async Task<IActionResult> Completions([FromBody] ChatRequest request, CancellationToken ct)
    {
        var profile = await profileService.GetByIdAsync(request.ProfileId);
        if (profile is null) return NotFound($"Profile '{request.ProfileId}' not found.");

        var allItems  = new List<SanitizedItem>();
        string? sessionId = null;
        var sanitizedMessages = new List<ChatMessage>();

        foreach (var msg in request.Messages)
        {
            if (msg.Role != "user")
            {
                sanitizedMessages.Add(msg);
                continue;
            }

            var result = sanitizerService.Sanitize(msg.Content, profile);
            sessionId ??= result.SessionId;
            allItems.AddRange(result.Items);
            sanitizedMessages.Add(new ChatMessage(msg.Role, result.SanitizedText));
        }

        string assistantMessage;
        try
        {
            assistantMessage = await llmProxy.ChatAsync(request.LlmConfig, sanitizedMessages, ct);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { Error = "LLM provider error.", Details = ex.Message });
        }

        return Ok(new ChatResponse
        {
            SessionId        = sessionId ?? string.Empty,
            SanitizedRequest = sanitizedMessages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty,
            AssistantMessage = assistantMessage,
            SanitizedItems   = allItems
        });
    }
}
