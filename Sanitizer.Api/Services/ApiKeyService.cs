using System.Security.Cryptography;
using System.Text;
using Sanitizer.Api.Models;
using Sanitizer.Api.Storage;

namespace Sanitizer.Api.Services;

public class ApiKeyService(IApiKeyStorage storage)
{
    /// <summary>Создаёт новый ключ. Открытый ключ возвращается единожды.</summary>
    public async Task<(ApiKey Model, string PlainKey)> CreateAsync(string name, DateTime? expiresAt = null)
    {
        var plain = $"sk-san-{Guid.NewGuid():N}";
        var model = new ApiKey
        {
            Name      = name,
            KeyHash   = Hash(plain),
            ExpiresAt = expiresAt,
            IsActive  = true
        };
        await storage.SaveAsync(model);
        return (model, plain);
    }

    public Task<List<ApiKey>> GetAllAsync() => storage.GetAllAsync();

    public async Task<bool> DeactivateAsync(string id)
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
        var hash = Hash(plainKey);
        var keys = await storage.GetAllAsync();
        return keys.Any(k =>
            k.IsActive &&
            k.KeyHash == hash &&
            (k.ExpiresAt is null || k.ExpiresAt > DateTime.UtcNow));
    }

    public static string Hash(string plain) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plain))).ToLowerInvariant();
}
