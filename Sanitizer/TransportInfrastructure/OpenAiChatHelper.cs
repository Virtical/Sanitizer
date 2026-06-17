using System.ClientModel;
using System.Net;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Sanitizer.TransportInfrastructure;

public static class OpenAiChatHelper
{
    private const int NetworkTimeoutInSeconds = 100;

    public static IChatClient CreateChatClient(OpenAiConfig openAiConfig, ProxyConfig? proxyConfig)
    {
        var openAiClientOptions = new OpenAIClientOptions
        {
            NetworkTimeout = TimeSpan.FromSeconds(NetworkTimeoutInSeconds)
        };
        
        if (proxyConfig is not null)
        {
            openAiClientOptions.Transport =
                new HttpProxyPipelineTransport(BuildProxy(proxyConfig), NetworkTimeoutInSeconds);
        }
            
        if (openAiConfig.Address != null)
        {
            openAiClientOptions.Endpoint = new Uri(openAiConfig.Address);
        }
        
        var chatClient = new OpenAIClient(new ApiKeyCredential(openAiConfig.ApiKey), openAiClientOptions)
            .GetChatClient(openAiConfig.Model)
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();
        
        return chatClient;
    }

    private static WebProxy BuildProxy(ProxyConfig proxyConfig)
    {
        ArgumentNullException.ThrowIfNull(proxyConfig);

        if (string.IsNullOrWhiteSpace(proxyConfig.Address))
            throw new ArgumentException("Proxy address is not configured.", nameof(proxyConfig));

        var proxy = new WebProxy(proxyConfig.Address)
        {
            Credentials = new NetworkCredential(
                proxyConfig.Login,
                proxyConfig.Password)
        };

        return proxy;
    }
}