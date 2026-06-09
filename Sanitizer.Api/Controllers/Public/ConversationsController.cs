#pragma warning disable OPENAI001
using System.ClientModel;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Conversations;
using Sanitizer.Api.Controllers.Client.Response;
using Sanitizer.Api.Controllers.Public.Client.Requests;
using Sanitizer.Api.Controllers.Public.Client.Responses;
using Sanitizer.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers.Public;

[ApiController]
[Route("proxy/v1/responses")]
public class ConversationsController(IHttpClientFactory httpClientFactory, ProfileService profileService, SanitizerService sanitizerService) : ControllerBase
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient("OpenAI");

    [HttpPost]
    [SwaggerOperation(Summary = "Create a conversation")]
    public async Task<ActionResult<CreateConversationResponse>> CreateConversationAsync([FromHeader(Name = "Authorization")]string authorization, [FromBody] CreateResponsesRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/responses");
        httpRequest.Headers.Add("Authorization", authorization);

        var profile = await profileService.GetByNameAsync("Debug");
        var sanitization = sanitizerService.Sanitize(request.Input, profile!);
        request.Input = sanitization.SanitizedText;

        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        
        var response = await httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        return response.IsSuccessStatusCode
            ? Ok(responseBody)
            : StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<JsonElement>(responseBody));
    }
}
