using Sanitizer.Api.Auth;
using Sanitizer.Api.Controllers.Internal.Client.Requests;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;
using Sanitizer.Api.Services;
using Sanitizer.Api.Storage;

namespace Sanitizer;

public static class Configurator
{
    public static async Task AddDefaultUsers(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var usersService = scope.ServiceProvider.GetRequiredService<UsersService>();
        await usersService.RegisterAsync("Test1", "Test123");
        await usersService.RegisterAsync("Test2", "Test123");
    }

    public static async Task AddDefaultProfiles(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IProfileStorage>();
        var apiKeyContext = new CurrentApiKeyContext();
        apiKeyContext.SetApiKeyId(Guid.Empty);
        var profileService = new ProfileService(storage, apiKeyContext);

        await profileService.CreateAsync(new CreateProfileRequest(
            "LLM-safe",
            new Dictionary<DetectorType, StrategyConfig>
            {
                { DetectorType.Email, StrategyType.Tokenize },
                { DetectorType.Phone, StrategyType.Tokenize },
                { DetectorType.Card, StrategyType.Remove },
                { DetectorType.ApiKey, StrategyType.Remove },
                { DetectorType.IpAddress, StrategyType.Mask },
                { DetectorType.Guid, StrategyType.Tokenize },
                { DetectorType.Name, StrategyType.Tokenize },
                { DetectorType.Url, StrategyType.Tokenize },
            }));
        
        await profileService.CreateAsync(new CreateProfileRequest(
            "Logging",
            new Dictionary<DetectorType, StrategyConfig>
            {
                { DetectorType.Email, StrategyType.Mask },
                { DetectorType.Phone, StrategyType.Mask },
                { DetectorType.Card, StrategyType.Remove },
                { DetectorType.ApiKey, StrategyType.Mask },
                { DetectorType.IpAddress, StrategyType.Mask },
                { DetectorType.Name, StrategyType.Mask },
                { DetectorType.Url, StrategyType.Remove },
            }));
        
        await profileService.CreateAsync(new CreateProfileRequest(
            "Support",
            new Dictionary<DetectorType, StrategyConfig>
            {
                { DetectorType.Email, StrategyType.Tokenize },
                { DetectorType.Phone, StrategyType.Mask },
                { DetectorType.Card, StrategyType.Remove },
                { DetectorType.ApiKey, StrategyType.Remove },
                { DetectorType.IpAddress, StrategyType.Tokenize },
                { DetectorType.Guid, StrategyType.Tokenize },
                { DetectorType.Name, StrategyType.Tokenize },
                { DetectorType.Url, StrategyType.Tokenize },
            }));
        
        await profileService.CreateAsync(new CreateProfileRequest(
            "Analytics",
            new Dictionary<DetectorType, StrategyConfig>
            {
                { DetectorType.Email, StrategyType.Hash },
                { DetectorType.Phone, StrategyType.Hash },
                { DetectorType.Card, StrategyType.Remove },
                { DetectorType.ApiKey, StrategyType.Remove },
                { DetectorType.IpAddress, StrategyType.Mask },
                { DetectorType.Guid, StrategyType.Hash },
                { DetectorType.Name, StrategyType.Hash },
                { DetectorType.Url, StrategyType.Remove },
            }));
        
        await profileService.CreateAsync(new CreateProfileRequest(
            "Debug",
            new Dictionary<DetectorType, StrategyConfig>
            {
                { DetectorType.Email, StrategyType.Tokenize },
                { DetectorType.Phone, StrategyType.Tokenize },
                { DetectorType.Card, StrategyType.Tokenize },
                { DetectorType.ApiKey, StrategyType.Tokenize },
                { DetectorType.IpAddress, StrategyType.Tokenize },
                { DetectorType.Guid, StrategyType.Tokenize },
                { DetectorType.Name, StrategyType.Tokenize },
                { DetectorType.Url, StrategyType.Tokenize },
            }));
    }
}