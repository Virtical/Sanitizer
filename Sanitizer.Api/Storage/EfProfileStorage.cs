using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public class EfProfileStorage(SanitizerDbContext db) : IProfileStorage
{
    public async Task<SanitizationProfileEntity[]> GetAllAsync()
    {
        return await db.Profiles
            .Include(p => p.Rules)
            .AsNoTracking()
            .ToArrayAsync();
    }

    public async Task<SanitizationProfileEntity?> GetByIdAsync(string id)
    {
        return await db.Profiles
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task SaveAsync(SanitizationProfileEntity entity)
    {
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
            db.Rules.RemoveRange(existing.Rules);
            existing.Rules = entity.Rules;
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
}
