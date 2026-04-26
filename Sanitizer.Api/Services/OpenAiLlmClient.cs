using Sanitizer.Api.Models;

namespace Sanitizer.Api.Services;

public class OpenAiLlmClient(LlmProxyService proxy, IConfiguration config) : ILlmClient
{
    public Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default)
    {
        var llmConfig = new LlmConfig
        {
            ApiKey  = config["Llm:ApiKey"]  ?? string.Empty,
            Model   = config["Llm:Model"]   ?? "gpt-4o-mini",
            BaseUrl = config["Llm:BaseUrl"] ?? "https://api.openai.com"
        };

        return proxy.ChatAsync(llmConfig, [new ChatMessage("user", prompt)], ct);
    }
}
