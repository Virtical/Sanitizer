using System.Net.Http.Headers;
using System.Text.Json;
using Sanitizer.Api.Models;

namespace Sanitizer.Api.Services;

/// <summary>Прокси к любому OpenAI-совместимому LLM API.</summary>
public class LlmProxyService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public async Task<string> ChatAsync(LlmConfig config, List<ChatMessage> messages,
        CancellationToken ct = default)
    {
        var baseUrl = config.BaseUrl.TrimEnd('/');
        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);

        var body = new
        {
            model    = config.Model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content })
        };

        request.Content = JsonContent.Create(body, options: JsonOpts);
        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"LLM provider returned {(int)response.StatusCode}: {err}");
        }

        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}
