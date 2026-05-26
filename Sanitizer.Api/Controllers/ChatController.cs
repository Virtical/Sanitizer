using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Controllers.Client.Response;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Message;
using Sanitizer.Api.Services;
using Sanitizer.Api.ToolCalls;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers;

/// <summary>
/// LLM Proxy: санитизирует входящие сообщения по профилю,
/// проксирует запрос к LLM, возвращает ответ вместе со списком замен.
/// </summary>
[ApiController]
[Route("api/chat")]
public class ChatController(
    SanitizerService sanitizerService,
    ProfileService profileService,
    DesanitizerService desanitizer,
    ChatHistoryService chatHistoryService,
    IChatClient chatClient) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Получение названий диалогов")]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await chatHistoryService.GetAllAsync());

    [HttpPost]
    [SwaggerOperation(Summary = "Создание диалога")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateChatRequest request)
        => Ok((await chatHistoryService.SaveChatAsync(request.Name)).Id);

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Получение сообщений диалога")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] string id)
    {
        var chat = await chatHistoryService.GetByIdAsync(id);
        return chat is null ? NotFound() : Ok(chat);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Изменение диалога")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] string id,
        [FromBody] UpdateChatRequest request)
        => Ok(await chatHistoryService.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Удаление диалога")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute] string id)
    {
        var deleted = await chatHistoryService.DeleteChatAsync(id);
        return deleted ? Ok(id) : NotFound();
    }

    [HttpPost("send/{id}")]
    [SwaggerOperation(Summary = "Отправка сообщений")]
    public async Task Send([FromRoute] string id, [FromBody] SendMessageRequest messageRequest)
    {
        if (string.IsNullOrEmpty(id))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("id is required");
            return;
        }

        if (string.IsNullOrEmpty(messageRequest.Message))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("Message is required");
            return;
        }

        var message = await chatHistoryService.AddMessageAsync(
            id, MessageRequest.CreateSent(messageRequest.Message));

        SanitizationResult sanitization;
        try
        {
            var profileId = await chatHistoryService.GetProfileIdAsync(id);
            ProfileResponse profile = profileId is not null
                ? await profileService.GetByIdAsync(profileId) ?? ProfileResponse.Default
                : ProfileResponse.Default;

            sanitization = sanitizerService.Sanitize(messageRequest.Message, profile);
            await chatHistoryService.AddMessageAsync(
                id, MessageRequest.CreateSanitized(sanitization.SanitizedText, message.Id));
        }
        catch (InvalidOperationException ex)
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(ex.Message);
            return;
        }

        string raw;
        try
        {
            Response.Headers.Append("X-Chat-Id", id);
            Response.ContentType = "text/plain";
            Response.Headers.Append("Cache-Control", "no-cache");
            
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, "Ты полезный ассистент"),
                new(ChatRole.User, sanitization.SanitizedText)
            };
            
            var options = new ChatOptions
            {
                Tools = ChatFunctionFactory.CreateTools()
            };

            var fullResponse = new StringBuilder();
            
            await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    await Response.WriteAsync(update.Text);
                    await Response.Body.FlushAsync();
                    fullResponse.Append(update.Text);
                }
            }

            raw = fullResponse.ToString();
        }
        catch (Exception ex)
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(ex.Message);
            return;
        }

        var final = desanitizer.Desanitize(raw, sanitization.Context);
        await chatHistoryService.AddMessageAsync(id, MessageRequest.CreateAnswer(final));
    }
}
