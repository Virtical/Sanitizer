using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;
using Sanitizer.Api.Services;

namespace Sanitizer;

public static class Configurator
{
    public static async Task AddDefaultProfiles(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var profileService = scope.ServiceProvider.GetRequiredService<ProfileService>();

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