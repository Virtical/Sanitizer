using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Chat;

namespace Sanitizer.Api.Services;

public class OpenAiLlmClient(LlmProxyService proxy, IConfiguration config) : ILlmClient
{
    public Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default)
    {
        var llmConfig = new LlmConfig
        {
            ApiKey  = config["Llm:ApiKey"]  ?? throw new NullReferenceException(),
            Model   = config["Llm:Model"]   ?? throw new NullReferenceException(),
            BaseUrl = config["Llm:BaseUrl"] ?? throw new NullReferenceException()
        };

        return proxy.ChatAsync(llmConfig, [new ChatMessage("user", prompt)], ct);
    }
}
