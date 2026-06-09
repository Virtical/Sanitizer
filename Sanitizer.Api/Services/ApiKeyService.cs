using System.Security.Cryptography;
using System.Text;
using Sanitizer.Api.Models;
using Sanitizer.Api.Storage;

namespace Sanitizer.Api.Services;

public class ApiKeyService(IApiKeyStorage storage)
{
    /// <summary>Создаёт новый ключ. Открытый ключ возвращается единожды.</summary>
    public async Task<ApiKey> CreateAsync(string name, DateTime? expiresAt = null)
    {
        var plain = Guid.NewGuid().ToString();
        var model = new ApiKey
        {
            Name      = name,
            KeyHash   = plain,
            ExpiresAt = expiresAt,
            IsActive  = true
        };
        await storage.SaveAsync(model);
        return model;
    }

    public Task<List<ApiKey>> GetAllAsync() => storage.GetAllAsync();

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var keys = await storage.GetAllAsync();
        var key  = keys.FirstOrDefault(k => k.Id == id);
        if (key is null) return false;
        key.IsActive = false;
        await storage.SaveAsync(key);
        return true;
    }

    public async Task<bool> IsValidAsync(string plainKey)
    {
        var key = await storage.GetAsync(plainKey);
        return key is not null && key.IsValid();
    }
    
    public async Task<ApiKey> GetTokenAsync(Guid id)
        => await storage.GetAsync(id);
}