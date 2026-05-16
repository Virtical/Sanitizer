using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Controllers.Client.Response;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;
using Sanitizer.Api.Storage;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Services;

public class ProfileService(IProfileStorage storage)
{
    public async Task<ProfileResponse[]> GetAllAsync()
    {
        var entities = await storage.GetAllAsync();
        return entities.Select(MapToModel).ToArray();
    }
    
    public async Task<ProfileResponse> GetByIdAsync(string id)
    {
        var existing = await storage.GetByIdAsync(id);
        if (existing is null)
        {
            throw new Exception($"Profile with id={id} not found");
        }
        
        return MapToModel(existing);
    }

    public async Task CreateAsync(CreateProfileRequest profileRequest)
    {
        var id = Guid.NewGuid().ToString();
        var entity = new SanitizationProfileEntity
        {
            Id = id,
            Name = profileRequest.Name,
            Rules = profileRequest.Rules.Select(r => new SanitizationRuleEntity
            {
                ProfileId = id,
                DetectorType = r.Key,
                StrategyType = r.Value.Strategy,
            }).ToList()
        };

        await storage.SaveAsync(entity);
    }

    public async Task<ProfileResponse[]> UpdateAsync(string id, UpdateProfileRequest updated)
    {
        var existing = await storage.GetByIdAsync(id);
        if (existing is null)
        {
            throw new Exception($"Profile with id={id} not found");
        }
        
        var entity = new SanitizationProfileEntity
        {
            Id = id,
            Name = updated.Name,
            Rules = updated.Rules.Select(r => new SanitizationRuleEntity
            {
                ProfileId = id,
                DetectorType = r.Key,
                StrategyType = r.Value.Strategy,
            }).ToList()
        };

        await storage.SaveAsync(entity);
        return await GetAllAsync();
    }

    public Task<bool> DeleteAsync(string id) => storage.DeleteAsync(id);
    
    private static ProfileResponse MapToModel(SanitizationProfileEntity e)
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
            };
        }

        return model;
    }
}
