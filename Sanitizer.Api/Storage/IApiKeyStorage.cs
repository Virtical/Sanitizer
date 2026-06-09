using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public interface IApiKeyStorage
{
    Task<List<ApiKey>> GetAllAsync();
    Task<ApiKey> GetAsync(Guid id);
    Task<ApiKey?> GetAsync(string token);
    Task SaveAsync(ApiKey key);
}
