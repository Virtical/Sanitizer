using Sanitizer.Api.Controllers.Public.Client.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace Sanitizer.Api.Swagger;

public class CreateConversationRequestExample : IExamplesProvider<CreateResponsesRequest>
{
    public CreateResponsesRequest GetExamples()
    {
        return new CreateResponsesRequest
        {
            Model = "gpt-5.4",
            Input = "Tell me a three sentence bedtime story about a unicorn."
        };
    }
}