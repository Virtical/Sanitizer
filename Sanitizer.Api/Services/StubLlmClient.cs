namespace Sanitizer.Api.Services;

public class StubLlmClient : ILlmClient
{
    public Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default)
        => Task.FromResult($"[STUB-LLM] Ответ на: {prompt}");
}
