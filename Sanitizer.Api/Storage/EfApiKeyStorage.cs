using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Data;
using Sanitizer.Api.Data.Entities;
using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public class EfApiKeyStorage(SanitizerDbContext db) : IApiKeyStorage
{
    public async Task<List<ApiKey>> GetAllAsync()
    {
        var entities = await db.ApiKeys.AsNoTracking().ToListAsync();
        return entities.Select(MapToModel).ToList();
    }

    public async Task SaveAsync(ApiKey key)
    {
        var existing = await db.ApiKeys.FirstOrDefaultAsync(k => k.Id == key.Id);
        if (existing is null)
        {
            db.ApiKeys.Add(MapToEntity(key));
        }
        else
        {
            existing.Name = key.Name;
            existing.KeyHash = key.KeyHash;
            existing.CreatedDate = key.CreatedDate;
            existing.ExpiresAt = key.ExpiresAt;
            existing.IsActive = key.IsActive;
        }

        await db.SaveChangesAsync();
    }

    private static ApiKey MapToModel(ApiKeyEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        KeyHash = e.KeyHash,
        CreatedDate = e.CreatedDate,
        ExpiresAt = e.ExpiresAt,
        IsActive = e.IsActive
    };

    private static ApiKeyEntity MapToEntity(ApiKey m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        KeyHash = m.KeyHash,
        CreatedDate = m.CreatedDate,
        ExpiresAt = m.ExpiresAt,
        IsActive = m.IsActive
    };
}
