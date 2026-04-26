namespace Sanitizer.Api.Services;

public interface ILlmClient
{
    Task<string> GetCompletionAsync(string prompt, CancellationToken ct = default);
}
