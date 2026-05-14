using Sanitizer.Api.Models;
using Sanitizer.Api.Storage;

namespace Sanitizer.Api.Services;

public class ProfileService(IProfileStorage storage)
{
    public Task<List<SanitizationProfile>> GetAllAsync() =>
        storage.GetAllAsync();

    public async Task<SanitizationProfile> GetByIdAsync(string? id)
    {
        if (id is null)
        {
            return new SanitizationProfile();
        }
        var r = (await storage.GetAllAsync()).FirstOrDefault(p => p.Id == id);
        return r ?? new SanitizationProfile();
    }


    public async Task<SanitizationProfile> CreateAsync(SanitizationProfile profile)
    {
        profile.Id = Guid.NewGuid().ToString();
        await storage.SaveAsync(profile);
        return profile;
    }

    public async Task<SanitizationProfile?> UpdateAsync(string id, SanitizationProfile updated)
    {
        var existing = await GetByIdAsync(id);

        updated.Id = id;
        await storage.SaveAsync(updated);
        return updated;
    }

    public Task<bool> DeleteAsync(string id) => storage.DeleteAsync(id);
}
