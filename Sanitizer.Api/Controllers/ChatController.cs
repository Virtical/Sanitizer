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
    
    [HttpPost("sanitized/{chatId}")]
    [SwaggerOperation(Summary = "Санитизировать сообщение")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] string chatId, [FromBody] SanitizedRequest request)
    {
        var profileId = await chatHistoryService.GetProfileIdAsync(chatId);
        var profile = profileId is not null
            ? await profileService.GetByIdAsync(profileId) ?? ProfileResponse.Default
            : ProfileResponse.Default;

        var sanitization = sanitizerService.Sanitize(request.Message, profile);
        
        return Ok(sanitization.SanitizedText);
    }

    [HttpPost("send/{chatId}")]
    [SwaggerOperation(Summary = "Отправка сообщений")]
    public async Task Send([FromRoute] string chatId, [FromBody] SendMessageRequest messageRequest)
    {
        if (string.IsNullOrEmpty(chatId))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("chatId is required");
            return;
        }

        if (string.IsNullOrEmpty(messageRequest.Message))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("Message is required");
            return;
        }

        var message = await chatHistoryService.AddMessageAsync(
            chatId, MessageRequest.CreateSent(messageRequest.Message));

        SanitizationResult sanitization;
        try
        {
            var profileId = await chatHistoryService.GetProfileIdAsync(chatId);
            var profile = profileId is not null
                ? await profileService.GetByIdAsync(profileId) ?? ProfileResponse.Default
                : ProfileResponse.Default;

            sanitization = sanitizerService.Sanitize(messageRequest.Message, profile);
            await chatHistoryService.AddMessageAsync(
                chatId, MessageRequest.CreateSanitized(sanitization.SanitizedText, message.Id));
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
            Response.Headers.Append("X-Chat-Id", chatId);
            Response.ContentType = "text/plain";
            Response.Headers.Append("Cache-Control", "no-cache");

            var chatSession = await chatHistoryService.GetByIdAsync(chatId);
            var messages = new List<ChatMessage>();

            if (chatSession is null)
            {
                messages.Add(new ChatMessage(ChatRole.System, "Ты полезный ассистент"));
                messages.Add(new ChatMessage(ChatRole.User, sanitization.SanitizedText));
            }
            else
            {
                messages.AddRange(chatSession.Messages.Select(ToChatMessage).OfType<ChatMessage>());
            }

            var options = new ChatOptions
            {
                Tools = ChatFunctionFactory.CreateTools()
            };

            var streaming = new StreamingDesanitizer(desanitizer, sanitization.Context);
            var fullResponse = new StringBuilder();

            await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options))
            {
                if (string.IsNullOrEmpty(update.Text)) continue;

                var safe = streaming.Push(update.Text);
                if (safe.Length > 0)
                {
                    await Response.WriteAsync(safe);
                    await Response.Body.FlushAsync();
                    fullResponse.Append(safe);
                }
            }

            var tail = streaming.Flush();
            if (tail.Length > 0)
            {
                await Response.WriteAsync(tail);
                await Response.Body.FlushAsync();
                fullResponse.Append(tail);
            }

            raw = fullResponse.ToString();
        }
        catch (Exception ex)
        {
            Response.StatusCode = 400;
            await Response.WriteAsync(ex.Message);
            return;
        }

        await chatHistoryService.AddMessageAsync(chatId, MessageRequest.CreateAnswer(raw));
    }

    private static ChatMessage? ToChatMessage(Message message)
        => message.Type switch
        {
            MessageType.Sent => new ChatMessage(ChatRole.User, message.Text),
            MessageType.Answer => new ChatMessage(ChatRole.Assistant, message.Text),
            _ => null
        };
}
