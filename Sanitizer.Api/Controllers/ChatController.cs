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
                             DesanitizerService desanitizer,
                             ChatHistoryService chatHistoryService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Получение названий диалогов")]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await chatHistoryService.GetAllAsync());
    
    [HttpGet("{chatId}")]
    [SwaggerOperation(Summary = "Получение сообщений диалога")]
    public async Task<IActionResult> GetByIdAsync([FromRoute]string chatId)
        => Ok(await chatHistoryService.GetByIdAsync(chatId));
    
    [HttpPost]
    [SwaggerOperation(Summary = "Создание диалога")]
    public async Task<IActionResult> CreateAsync([FromBody]string name)
        => Ok(await chatHistoryService.SaveChatAsync(name));
    
    [HttpPut("{chatId}")]
    [SwaggerOperation(Summary = "Изменения диалога")]
    public async Task<IActionResult> UpdateAsync([FromRoute]string chatId, [FromBody]UpdateRequest request)
        => Ok(await chatHistoryService.UpdateAsync(chatId, request));

    [SwaggerIgnore]
    [HttpDelete("{chatId}")]
    [SwaggerOperation(Summary = "Удаление диалога")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute]string chatId)
        => Ok(await chatHistoryService.DeleteChatAsync(chatId));
    
    [HttpPost("send")]
    [SwaggerOperation(Summary = "Отправка сообщений")]
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

        var message = await chatHistoryService.AddMessageAsync(request.ChatId, MessageRequest.CreateSent(request.Message));

        SanitizationResult sanitization;
        try
        {
            var profileId = await chatHistoryService.GetProfileIdAsync(request.ChatId);
            var profile = await profileService.GetByIdAsync(profileId);
            
            sanitization = sanitizerService.Sanitize(request.Message, profile);
            await chatHistoryService.AddMessageAsync(request.ChatId, MessageRequest.CreateSanitized(sanitization.SanitizedText, message.Id));
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
        await chatHistoryService.AddMessageAsync(request.ChatId, MessageRequest.CreateAnswer(final));

        return Ok(await chatHistoryService.GetByIdAsync(request.ChatId));
    }
}
