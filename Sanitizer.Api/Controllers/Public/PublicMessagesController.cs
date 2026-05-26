using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Sanitizer.Api.Controllers.Client.Response;
using Sanitizer.Api.Controllers.Public.Dto;
using Sanitizer.Api.Models;
using Sanitizer.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers.Public;

/// <summary>Отправка сообщений и история (публичный API).</summary>
[ApiController]
[Route("api/public/message")]
[Tags("Public – Messages")]
public class PublicMessagesController(
    SanitizerService sanitizerService,
    DesanitizerService desanitizer,
    IChatClient chatClient) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Отправить сообщение (стриминг)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Send([FromBody] SendMessageDto dto)
    {
        if (string.IsNullOrEmpty(dto.Message))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(new ErrorDto("INVALID_MESSAGE", "Сообщение не может быть пустым."));
            return;
        }

        SanitizationResult sanitization;
        try
        {
            var profile = new ProfileResponse
            {
                Id = string.Empty,
                Name = string.Empty,
                Rules = dto.Rules
            };

            sanitization = sanitizerService.Sanitize(dto.Message, profile);
        }
        catch (InvalidOperationException ex)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(new ErrorDto("SANITIZATION_ERROR", ex.Message));
            return;
        }
        
        try
        {
            Response.ContentType = "text/plain";
            Response.Headers.Append("Cache-Control", "no-cache");

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, "Ты полезный ассистент"),
                new(ChatRole.User, sanitization.SanitizedText)
            };

            var fullResponse = new StringBuilder();
            await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    await Response.WriteAsync(update.Text);
                    await Response.Body.FlushAsync();
                    fullResponse.Append(update.Text);
                }
            }

            var raw = fullResponse.ToString();
            var final = desanitizer.Desanitize(raw, sanitization.Context);
        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            await Response.WriteAsync(ex.Message);
        }
    }

    [HttpPost("sync")]
    [SwaggerOperation(Summary = "Отправить сообщение (синхронно, без стриминга)")]
    public async Task<IActionResult> SendSync([FromBody] SendMessageDto dto)
    {
        if (string.IsNullOrEmpty(dto.Message))
            return BadRequest(new ErrorDto("INVALID_MESSAGE", "Сообщение не может быть пустым."));

        SanitizationResult sanitization;
        try
        {
            var profile = new ProfileResponse
            {
                Id = string.Empty,
                Name = string.Empty,
                Rules = dto.Rules
            };
            sanitization = sanitizerService.Sanitize(dto.Message, profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorDto("SANITIZATION_ERROR", ex.Message));
        }
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "Ты полезный ассистент"),
            new(ChatRole.User, sanitization.SanitizedText)
        };

        var response = await chatClient.GetResponseAsync(messages);
        var final = desanitizer.Desanitize(response.Text, sanitization.Context);

        return Ok(new MessageResponseDto(sanitization.SanitizedText, final));
    }
}
