namespace Sanitizer.Api.Models;

public class LlmConfig
{
    /// <summary>Поддерживается любой OpenAI-совместимый API.</summary>
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Базовый URL провайдера (по умолчанию OpenAI).</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com";
}