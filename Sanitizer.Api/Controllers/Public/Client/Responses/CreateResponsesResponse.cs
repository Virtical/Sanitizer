using System.Text.Json.Serialization;

namespace Sanitizer.Api.Controllers.Public.Client.Responses;

public class CreateConversationResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}