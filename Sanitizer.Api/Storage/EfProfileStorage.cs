using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public class EfProfileStorage(SanitizerDbContext db) : IProfileStorage
{
    public async Task<SanitizationProfileEntity[]> GetAllAsync(Guid apiKeyId)
    {
        return await db.Profiles
            .Include(p => p.Rules)
            .AsNoTracking()
            .Where(p => p.ApiKeyId == apiKeyId)
            .ToArrayAsync();
    }

    public async Task<SanitizationProfileEntity?> GetByIdAsync(string id, Guid apiKeyId)
    {
        return await db.Profiles
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == id && p.ApiKeyId == apiKeyId);
    }

    public async Task SaveAsync(SanitizationProfileEntity entity)
    {
        // Ищем без фильтра по ApiKeyId — владелец уже зафиксирован при создании
        var existing = await db.Profiles
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == entity.Id);

        if (existing is null)
        {
            db.Profiles.Add(entity);
        }
        else
        {
            existing.Name = entity.Name;
            existing.ApiKeyId = entity.ApiKeyId;
            db.Rules.RemoveRange(existing.Rules);
            existing.Rules = entity.Rules;
        }

        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string id, Guid apiKeyId)
    {
        var existing = await db.Profiles
            .FirstOrDefaultAsync(p => p.Id == id && p.ApiKeyId == apiKeyId);

        if (existing is null) return false;

        db.Profiles.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
