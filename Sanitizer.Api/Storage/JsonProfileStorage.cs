using System.Text.Json;
using Sanitizer.Api.Models;

namespace Sanitizer.Api.Storage;

public class JsonProfileStorage : IProfileStorage
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOpts =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public JsonProfileStorage(IConfiguration config)
    {
        var dir = config["Storage:DataDirectory"] ?? "data";
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "profiles.json");
    }

    public async Task<List<SanitizationProfile>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try   { return await ReadAsync(); }
        finally { _lock.Release(); }
    }

    public async Task SaveAsync(SanitizationProfile profile)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAsync();
            var idx = all.FindIndex(p => p.Id == profile.Id);
            if (idx >= 0) all[idx] = profile; else all.Add(profile);
            await WriteAsync(all);
        }
        finally { _lock.Release(); }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            var all     = await ReadAsync();
            var removed = all.RemoveAll(p => p.Id == id);
            await WriteAsync(all);
            return removed > 0;
        }
        finally { _lock.Release(); }
    }

    private async Task<List<SanitizationProfile>> ReadAsync()
    {
        if (!File.Exists(_filePath)) return new();
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<SanitizationProfile>>(json, JsonOpts) ?? new();
    }

    private Task WriteAsync(List<SanitizationProfile> list) =>
        File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(list, JsonOpts));
}
