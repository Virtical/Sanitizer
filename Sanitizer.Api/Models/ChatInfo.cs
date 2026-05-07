namespace Sanitizer.Api.Models;

public class ChatInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Название чата (обычно — первое сообщение пользователя, до 30 символов).</summary>
    public string Name { get; set; } = "Новый диалог";
}
