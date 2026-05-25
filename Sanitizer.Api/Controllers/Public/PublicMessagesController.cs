using System.Text;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using Sanitizer.Api.Controllers.Client.Response;
using Sanitizer.Api.Controllers.Public.Dto;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Message;
using Sanitizer.Api.Services;
using Swashbuckle.AspNetCore.Annotations;
using AiChatMessage = OpenAI.Chat.ChatMessage;

namespace Sanitizer.Api.Controllers.Public;

/// <summary>Отправка сообщений и история (публичный API).</summary>
[ApiController]
[Route("api/public/v1/chats/{id}/messages")]
[Tags("Public – Messages")]
public class PublicMessagesController(
    SanitizerService sanitizerService,
    ProfileService profileService,
    DesanitizerService desanitizer,
    ChatHistoryService chatHistoryService,
    ChatClient openAiClient) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Получить историю сообщений чата")]
    [ProducesResponseType(typeof(MessageDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(
        [FromRoute] string id,
        [FromQuery] int limit = 50,
        [FromQuery] string? beforeId = null)
    {
        var chat = await chatHistoryService.GetByIdAsync(id);
        if (chat is null) return NotFound();

        var messages = chat.Messages.AsEnumerable();

        if (beforeId is not null)
        {
            var list = messages.ToList();
            var beforeIndex = list.FindIndex(m => m.Id == beforeId);
            if (beforeIndex >= 0)
                messages = list.Take(beforeIndex);
        }

        var result = messages
            .TakeLast(limit)
            .Select(m => new MessageDto(
                m.Id,
                m.Type.ToString().ToLowerInvariant(),
                m.Text,
                m.OriginalMessageId,
                DateTime.UtcNow))
            .ToArray();

        return Ok(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Отправить сообщение (стриминг)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Send([FromRoute] string id, [FromBody] SendMessageDto dto)
    {
        if (string.IsNullOrEmpty(id))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(new ErrorDto("INVALID_ID", "id обязателен."));
            return;
        }

        if (string.IsNullOrEmpty(dto.Message))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(new ErrorDto("INVALID_MESSAGE", "Сообщение не может быть пустым."));
            return;
        }

        var chat = await chatHistoryService.GetByIdAsync(id);
        if (chat is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            await Response.WriteAsJsonAsync(new ErrorDto("CHAT_NOT_FOUND", "Чат не найден."));
            return;
        }

        var message = await chatHistoryService.AddMessageAsync(
            id, MessageRequest.CreateSent(dto.Message));

        SanitizationResult sanitization;
        try
        {
            var profileId = await chatHistoryService.GetProfileIdAsync(id);
            ProfileResponse profile = profileId is not null
                ? await profileService.GetByIdAsync(profileId) ?? ProfileResponse.Default
                : ProfileResponse.Default;

            sanitization = sanitizerService.Sanitize(dto.Message, profile);
            await chatHistoryService.AddMessageAsync(
                id, MessageRequest.CreateSanitized(sanitization.SanitizedText, message.Id));
        }
        catch (InvalidOperationException ex)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(new ErrorDto("SANITIZATION_ERROR", ex.Message));
            return;
        }

        string raw;
        try
        {
            Response.Headers.Append("X-Chat-Id", id);
            Response.Headers.Append("X-Message-Id", message.Id);
            Response.ContentType = "text/plain";
            Response.Headers.Append("Cache-Control", "no-cache");

            var aiMessages = new List<AiChatMessage>
            {
                AiChatMessage.CreateSystemMessage("Ты полезный ассистент"),
                AiChatMessage.CreateUserMessage(sanitization.SanitizedText)
            };

            var fullResponse = new StringBuilder();
            await foreach (var update in openAiClient.CompleteChatStreamingAsync(aiMessages))
            {
                foreach (var content in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(content.Text))
                    {
                        await Response.WriteAsync(content.Text);
                        await Response.Body.FlushAsync();
                        fullResponse.Append(content.Text);
                    }
                }
            }

            raw = fullResponse.ToString();
        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            await Response.WriteAsync(ex.Message);
            return;
        }

        var final = desanitizer.Desanitize(raw, sanitization.Context);
        await chatHistoryService.AddMessageAsync(id, MessageRequest.CreateAnswer(final));
    }

    [HttpPost("sync")]
    [SwaggerOperation(Summary = "Отправить сообщение (синхронно, без стриминга)")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendSync([FromRoute] string id, [FromBody] SendMessageDto dto)
    {
        if (string.IsNullOrEmpty(dto.Message))
            return BadRequest(new ErrorDto("INVALID_MESSAGE", "Сообщение не может быть пустым."));

        var chat = await chatHistoryService.GetByIdAsync(id);
        if (chat is null)
            return NotFound(new ErrorDto("CHAT_NOT_FOUND", "Чат не найден."));

        var message = await chatHistoryService.AddMessageAsync(
            id, MessageRequest.CreateSent(dto.Message));

        SanitizationResult sanitization;
        try
        {
            var profileId = await chatHistoryService.GetProfileIdAsync(id);
            ProfileResponse profile = profileId is not null
                ? await profileService.GetByIdAsync(profileId) ?? ProfileResponse.Default
                : ProfileResponse.Default;

            sanitization = sanitizerService.Sanitize(dto.Message, profile);
            await chatHistoryService.AddMessageAsync(
                id, MessageRequest.CreateSanitized(sanitization.SanitizedText, message.Id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorDto("SANITIZATION_ERROR", ex.Message));
        }

        var aiMessages = new List<AiChatMessage>
        {
            AiChatMessage.CreateSystemMessage("Ты полезный ассистент"),
            AiChatMessage.CreateUserMessage(sanitization.SanitizedText)
        };

        var fullResponse = new StringBuilder();
        await foreach (var update in openAiClient.CompleteChatStreamingAsync(aiMessages))
        {
            foreach (var content in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(content.Text))
                    fullResponse.Append(content.Text);
            }
        }

        var raw = fullResponse.ToString();
        var final = desanitizer.Desanitize(raw, sanitization.Context);
        var answerMessage = await chatHistoryService.AddMessageAsync(
            id, MessageRequest.CreateAnswer(final));

        return Ok(new MessageResponseDto(answerMessage.Id, final));
    }
}
