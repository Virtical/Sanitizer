using System.Security.Cryptography;
using System.Text;
using Sanitizer.Api.Models;

namespace Sanitizer.Api.Strategies;

/// <summary>
/// Заменяет значение его хешем SHA-256 или SHA-512.
/// Параметры: algorithm (sha256|sha512), encoding (hex|base64), length (default 16).
/// </summary>
public class HashStrategy : ISanitizationStrategy
{
    public string Apply(string value, DetectorType _, string __)
    {
        var bytes    = Encoding.UTF8.GetBytes(value);
        var hashBytes = SHA256.HashData(bytes);

        var full = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return full.Length > 16 ? full[..16] : full;
    }
}
