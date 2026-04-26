namespace Sanitizer.Api.Models;

public record ChatSendRequest(string ProfileId, string Message);

public record ChatSendResponse(
    string SanitizedPrompt,
    string LlmRawResponse,
    string FinalResponse,
    string? SessionId,
    IReadOnlyList<SanitizedItem> SanitizedItems);
