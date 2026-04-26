using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public interface IProfileStorage
{
    Task<List<SanitizationProfile>> GetAllAsync();
    Task SaveAsync(SanitizationProfile profile);
    Task<bool> DeleteAsync(string id);
}
