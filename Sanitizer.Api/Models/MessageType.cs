namespace Sanitizer.Api.Models;

public enum MessageType
{
    Sent,       // Отправлено пользователем
    Answer,     // Ответ системы/LLM
    Sanitized   // Санитизированная копия сообщения
}
