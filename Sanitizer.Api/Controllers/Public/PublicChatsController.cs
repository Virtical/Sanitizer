using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Controllers.Public.Dto;
using Sanitizer.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers.Public;

/// <summary>Управление чатами (публичный API).</summary>
[ApiController]
[Route("api/public/v1/chats")]
[Tags("Public – Chats")]
public class PublicChatsController(ChatHistoryService chatHistoryService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Получить список чатов токена")]
    [ProducesResponseType(typeof(ChatInfoDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var chats = await chatHistoryService.GetAllAsync();
        var dtos = chats
            .Select(c => new ChatInfoDto(c.Id, c.Name, c.ProfileId, c.CreatedAt))
            .ToArray();
        return Ok(dtos);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Создать чат")]
    [ProducesResponseType(typeof(ChatInfoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateChatDto dto)
    {
        var chat = await chatHistoryService.SaveChatAsync(dto.Name);

        if (dto.ProfileId is not null)
        {
            await chatHistoryService.UpdateAsync(chat.Id, new UpdateChatRequest(dto.ProfileId));
            // Перечитываем обновлённый чат
            var updated = await chatHistoryService.GetByIdAsync(chat.Id);
            if (updated is not null)
            {
                chat = new Sanitizer.Api.Models.Chat.ChatInfo
                {
                    Id = updated.Id,
                    Name = updated.Name,
                    ProfileId = updated.ProfileId,
                    CreatedAt = updated.CreatedAt
                };
            }
        }

        var result = new ChatInfoDto(chat.Id, chat.Name, chat.ProfileId, chat.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = chat.Id }, result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Получить чат с историей сообщений")]
    [ProducesResponseType(typeof(ChatDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var chat = await chatHistoryService.GetByIdAsync(id);
        if (chat is null) return NotFound();

        var messages = chat.Messages
            .Select(m => new MessageDto(
                m.Id,
                m.Type.ToString().ToLowerInvariant(),
                m.Text,
                m.OriginalMessageId,
                DateTime.UtcNow))
            .ToArray();

        var result = new ChatDetailDto(
            chat.Id,
            chat.Name,
            chat.ProfileId,
            chat.CreatedAt,
            messages);

        return Ok(result);
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Обновить чат (имя, профиль)")]
    [ProducesResponseType(typeof(ChatInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateChatDto dto)
    {
        var chat = await chatHistoryService.GetByIdAsync(id);
        if (chat is null) return NotFound();

        await chatHistoryService.UpdateAsync(id, new UpdateChatRequest(dto.ProfileId));

        var updated = await chatHistoryService.GetByIdAsync(id);
        if (updated is null) return NotFound();

        return Ok(new ChatInfoDto(updated.Id, updated.Name, updated.ProfileId, updated.CreatedAt));
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Удалить чат")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var deleted = await chatHistoryService.DeleteChatAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
