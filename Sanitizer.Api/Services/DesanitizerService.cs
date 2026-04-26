using Sanitizer.Api.Models;

namespace Sanitizer.Api.Services;

public class DesanitizerService(TokenStore tokenStore)
{
    public string Desanitize(string llmResponse, SanitizationContext? context)
    {
        if (context is null) return llmResponse;
        return tokenStore.RestoreAll(context.SessionId, llmResponse);
    }
}
