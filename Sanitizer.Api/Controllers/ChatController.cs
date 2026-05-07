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
                             ILlmClient llmClient,
                             DesanitizerService desanitizer,
                             ChatHistoryService chatHistoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await chatHistoryService.GetAllAsync());
    
    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute]string chatId)
        => Ok(await chatHistoryService.GetByIdAsync(chatId));
    
    [HttpPost("save")]
    public async Task<IActionResult> SaveChatAsync([FromBody]string name)
        => Ok(await chatHistoryService.SaveChatAsync(name));

    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute]string chatId)
        => Ok(await chatHistoryService.DeleteChatAsync(chatId));
    
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ChatSendRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ChatId))
        {
            return BadRequest("ProfileId is required.");
        }

        if (string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("Message is required.");
        }

        var profile = await profileService.GetByIdAsync(request.ChatId);
        if (profile is null)
        {
            return NotFound($"Profile '{request.ChatId}' not found.");
        }

        await chatHistoryService.AddMessageAsync(request.ChatId, new MessageRequest
        {
            Text = request.Message,
            Type = MessageType.Sent,
        });

        SanitizationResult sanitization;
        try
        {
            sanitization = sanitizerService.Sanitize(request.Message, profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        
        await chatHistoryService.AddMessageAsync(request.ChatId, new MessageRequest
        {
            Text = sanitization.SanitizedText,
            Type = MessageType.Sanitized,
        });

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
        await chatHistoryService.AddMessageAsync(request.ChatId, new MessageRequest
        {
            Text = final,
            Type = MessageType.Answer,
        });

        return Ok(await chatHistoryService.GetByIdAsync(request.ChatId));
    }
}
