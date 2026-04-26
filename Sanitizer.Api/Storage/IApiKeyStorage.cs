using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public interface IApiKeyStorage
{
    Task<List<ApiKey>> GetAllAsync();
    Task SaveAsync(ApiKey key);
}
