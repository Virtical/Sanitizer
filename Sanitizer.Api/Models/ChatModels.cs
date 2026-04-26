namespace Sanitizer.Api.Models;

public record ChatMessage(string Role, string Content);

public class ChatRequest
{
    /// <summary>ID профиля санитизации.</summary>
    public string ProfileId { get; set; } = string.Empty;

    public List<ChatMessage> Messages { get; set; } = new();
    public LlmConfig LlmConfig { get; set; } = new();
}

public class LlmConfig
{
    /// <summary>Поддерживается любой OpenAI-совместимый API.</summary>
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Базовый URL провайдера (по умолчанию OpenAI).</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com";
}

public class ChatResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string SanitizedRequest { get; set; } = string.Empty;
    public string AssistantMessage { get; set; } = string.Empty;
    public List<SanitizedItem> SanitizedItems { get; set; } = new();
}
