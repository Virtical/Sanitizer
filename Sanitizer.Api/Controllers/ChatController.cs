using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Controllers.Client.Response;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Chat;
using Sanitizer.Api.Models.Message;
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
    
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Получение сообщений диалога")]
    public async Task<IActionResult> GetByIdAsync([FromRoute]string id)
        => Ok(await chatHistoryService.GetByIdAsync(id));
    
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Изменение диалога")]
    public async Task<IActionResult> UpdateAsync([FromRoute] string id, [FromBody] UpdateChatRequest request)
        => Ok(await chatHistoryService.UpdateAsync(id, request));
    
    [SwaggerIgnore]
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Удаление диалога")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute]string id)
        => Ok(await chatHistoryService.DeleteChatAsync(id));

    [HttpPost("send")]
    [SwaggerOperation(Summary = "Отправка сообщений и создание чата")]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest messageRequest)
        => await Send(null, messageRequest);
    
    
    [HttpPost("send/{id}")]
    [SwaggerOperation(Summary = "Отправка сообщений")]
    public async Task<IActionResult> Send([FromRoute]string? id, [FromBody] SendMessageRequest messageRequest)
    {
        if (string.IsNullOrEmpty(messageRequest.Message))
        {
            return BadRequest("Message is required.");
        }

        if (id is null)
        {
            var name = messageRequest.Message.Length >= 27
                ? messageRequest.Message[..27] + "..."
                : messageRequest.Message;

            var chat = await chatHistoryService.SaveChatAsync(name);
            id = chat.Id;
        }

        var message = await chatHistoryService.AddMessageAsync(id, MessageRequest.CreateSent(messageRequest.Message));

        SanitizationResult sanitization;
        try
        {
            var profileId = await chatHistoryService.GetProfileIdAsync(id);
            ProfileResponse profile;
            if (profileId is not null)
            {
                profile = await profileService.GetByIdAsync(profileId);
            }
            else
            {
                profile = ProfileResponse.Default;
            }
            
            sanitization = sanitizerService.Sanitize(messageRequest.Message, profile);
            await chatHistoryService.AddMessageAsync(id, MessageRequest.CreateSanitized(sanitization.SanitizedText, message.Id));
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
        await chatHistoryService.AddMessageAsync(id, MessageRequest.CreateAnswer(final));

        return Ok(await chatHistoryService.GetByIdAsync(id));
    }
}
