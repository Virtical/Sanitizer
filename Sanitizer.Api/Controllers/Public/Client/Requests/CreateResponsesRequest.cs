using System.Text.Json.Serialization;

namespace Sanitizer.Api.Controllers.Public.Client.Requests;

public class CreateResponsesRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("input")]
    public required string Input { get; set; }
}