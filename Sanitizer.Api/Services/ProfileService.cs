using Sanitizer.Api.Auth;
using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Controllers.Client.Response;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;
using Sanitizer.Api.Storage;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Services;

public class ProfileService(IProfileStorage storage, ICurrentApiKeyContext apiKeyContext)
{
    public async Task<ProfileResponse[]> GetAllAsync()
    {
        var entities = await storage.GetAllAsync(apiKeyContext.ApiKeyId);
        return entities.Select(MapToResponse).ToArray();
    }

    public async Task<ProfileResponse?> GetByIdAsync(string id)
    {
        var existing = await storage.GetByIdAsync(id, apiKeyContext.ApiKeyId);
        return existing is null ? null : MapToResponse(existing);
    }

    public async Task CreateAsync(CreateProfileRequest profileRequest)
    {
        foreach (var (type, cfg) in profileRequest.Rules)
        {
            if (type != DetectorType.Regex) continue;
            if (string.IsNullOrWhiteSpace(cfg.Pattern))
                throw new InvalidOperationException("Для типа Regex необходимо указать шаблон.");
            try { _ = new System.Text.RegularExpressions.Regex(cfg.Pattern); }
            catch (ArgumentException) { throw new InvalidOperationException("Некорректное регулярное выражение."); }
        }

        var id = Guid.NewGuid().ToString();
        var entity = new SanitizationProfileEntity
        {
            Id = id,
            Name = profileRequest.Name,
            ApiKeyId = apiKeyContext.ApiKeyId,
            Rules = profileRequest.Rules.Select(r => new SanitizationRuleEntity
            {
                ProfileId = id,
                DetectorType = r.Key,
                StrategyType = r.Value.Strategy,
                Pattern = r.Value.Pattern,
            }).ToList()
        };

        await storage.SaveAsync(entity);
    }

    public async Task<ProfileResponse?> UpdateAsync(string id, UpdateProfileRequest updated)
    {
        foreach (var (type, cfg) in updated.Rules)
        {
            if (type != DetectorType.Regex) continue;
            if (string.IsNullOrWhiteSpace(cfg.Pattern))
                throw new InvalidOperationException("Для типа Regex необходимо указать шаблон.");
            try { _ = new System.Text.RegularExpressions.Regex(cfg.Pattern); }
            catch (ArgumentException) { throw new InvalidOperationException("Некорректное регулярное выражение."); }
        }

        var existing = await storage.GetByIdAsync(id, apiKeyContext.ApiKeyId);
        if (existing is null) return null;

        var entity = new SanitizationProfileEntity
        {
            Id = id,
            Name = updated.Name,
            ApiKeyId = apiKeyContext.ApiKeyId,
            Rules = updated.Rules.Select(r => new SanitizationRuleEntity
            {
                ProfileId = id,
                DetectorType = r.Key,
                StrategyType = r.Value.Strategy,
                Pattern = r.Value.Pattern,
            }).ToList()
        };

        await storage.SaveAsync(entity);
        return await GetByIdAsync(id);
    }

    public Task<bool> DeleteAsync(string id) =>
        storage.DeleteAsync(id, apiKeyContext.ApiKeyId);

    private static ProfileResponse MapToResponse(SanitizationProfileEntity e)
    {
        var model = new ProfileResponse
        {
            Id = e.Id,
            Name = e.Name,
            Rules = new Dictionary<DetectorType, StrategyConfig>()
        };

        foreach (var r in e.Rules)
        {
            model.Rules[r.DetectorType] = new StrategyConfig
            {
                Strategy = r.StrategyType,
                Pattern = r.Pattern,
            };
        }

        return model;
    }
}
