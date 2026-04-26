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
    public string Apply(string value, DetectorType _, Dictionary<string, string> parameters, string __)
    {
        var algorithm = parameters.GetValueOrDefault("algorithm", "sha256");
        var encoding  = parameters.GetValueOrDefault("encoding",  "hex");
        var length    = int.Parse(parameters.GetValueOrDefault("length", "16"));

        var bytes    = Encoding.UTF8.GetBytes(value);
        var hashBytes = algorithm == "sha512" ? SHA512.HashData(bytes) : SHA256.HashData(bytes);

        var full = encoding == "base64"
            ? Convert.ToBase64String(hashBytes)
            : Convert.ToHexString(hashBytes).ToLowerInvariant();

        return full.Length > length ? full[..length] : full;
    }
}
