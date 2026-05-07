namespace Sanitizer.Api.Models;

public record ChatSendRequest(string ChatId, string Message);

public record ChatSendResponse(string SanitizedPrompt, string Response);
