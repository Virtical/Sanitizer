using System.Text.Json;
using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public class JsonApiKeyStorage : IApiKeyStorage
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOpts =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public JsonApiKeyStorage(IConfiguration config)
    {
        var dir = config["Storage:DataDirectory"] ?? "data";
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "apikeys.json");
    }

    public async Task<List<ApiKey>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try   { return await ReadAsync(); }
        finally { _lock.Release(); }
    }

    public async Task SaveAsync(ApiKey key)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAsync();
            var idx = all.FindIndex(k => k.Id == key.Id);
            if (idx >= 0) all[idx] = key; else all.Add(key);
            await WriteAsync(all);
        }
        finally { _lock.Release(); }
    }

    private async Task<List<ApiKey>> ReadAsync()
    {
        if (!File.Exists(_filePath)) return new();
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<ApiKey>>(json, JsonOpts) ?? new();
    }

    private Task WriteAsync(List<ApiKey> list) =>
        File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(list, JsonOpts));
}
