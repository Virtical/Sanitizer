using Sanitizer.Api.Controllers.Public.Client.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace Sanitizer.Api.Swagger;

public class ConversationResponseExample : IExamplesProvider<CreateConversationResponse>
{
    public CreateConversationResponse GetExamples()
    {
        return new CreateConversationResponse
        {
            Id = "conv_123",
            Object = "conversation",
            CreatedAt = 1741900000,
            Metadata = new Dictionary<string, string>
            {
                { "topic", "demo" }
            }
        };
    }
}