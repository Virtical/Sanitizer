namespace Sanitizer.Api.Models;

public record ChatSendRequest(string ProfileId, string Message);

public record ChatSendResponse(string SanitizedPrompt, string Response);
