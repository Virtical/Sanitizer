using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Data;
using Sanitizer.Api.Data.Entities;
using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public class EfProfileStorage(SanitizerDbContext db) : IProfileStorage
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<SanitizationProfile>> GetAllAsync()
    {
        var entities = await db.Profiles
            .Include(p => p.Rules)
            .AsNoTracking()
            .ToListAsync();

        return entities.Select(MapToModel).ToList();
    }

    public async Task SaveAsync(SanitizationProfile profile)
    {
        var existing = await db.Profiles
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == profile.Id);

        if (existing is null)
        {
            db.Profiles.Add(MapToEntity(profile));
        }
        else
        {
            existing.Name = profile.Name;
            existing.Description = profile.Description;
            existing.CreatedDate = profile.CreatedDate;
            existing.Reversible = profile.Reversible;

            db.Rules.RemoveRange(existing.Rules);
            existing.Rules = profile.Rules.Select(r => new SanitizationRuleEntity
            {
                ProfileId = existing.Id,
                DetectorType = r.Key,
                StrategyType = r.Value.Strategy,
                ParametersJson = JsonSerializer.Serialize(r.Value.Parameters ?? new(), JsonOpts)
            }).ToList();
        }

        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await db.Profiles.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return false;
        db.Profiles.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }

    private static SanitizationProfile MapToModel(SanitizationProfileEntity e)
    {
        var model = new SanitizationProfile
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            CreatedDate = e.CreatedDate,
            Reversible = e.Reversible,
            Rules = new()
        };

        foreach (var r in e.Rules)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(r.ParametersJson, JsonOpts) ?? new();
            model.Rules[r.DetectorType] = new StrategyConfig
            {
                Strategy = r.StrategyType,
                Parameters = dict
            };
        }

        return model;
    }

    private static SanitizationProfileEntity MapToEntity(SanitizationProfile p)
    {
        var e = new SanitizationProfileEntity
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedDate = p.CreatedDate,
            Reversible = p.Reversible,
        };

        e.Rules = p.Rules.Select(r => new SanitizationRuleEntity
        {
            ProfileId = e.Id,
            DetectorType = r.Key,
            StrategyType = r.Value.Strategy,
            ParametersJson = JsonSerializer.Serialize(r.Value.Parameters ?? new(), JsonOpts)
        }).ToList();

        return e;
    }
}
