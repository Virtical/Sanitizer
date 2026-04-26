using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class ApiKeyDetectorCases
{
    private static readonly ApiKeyDetector detector = new();

    public static IEnumerable<object[]> Valid =>
    [
        // OpenAI
        [detector, "sk-abcdefghijklmnopqrstuvwxyz123456", "sk-abcdefghijklmnopqrstuvwxyz123456"],
        [detector, "key is sk-ABCDEFGHIJKLMNOPQRSTU1234567890 here", "sk-ABCDEFGHIJKLMNOPQRSTU1234567890"],
        // GitHub
        [detector, "ghp_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcd", "ghp_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcd"],
        [detector, "gho_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmn", "gho_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmn"],
        // Prefix patterns
        [detector, "api_key=mysecretapikey123", "api_key=mysecretapikey123"],
        [detector, "token=secrettoken789secure", "token=secrettoken789secure"],
        [detector, "secret=mysupersecretvalue1", "secret=mysupersecretvalue1"],
        [detector, "access_token=Bearer_xyz123456789", "access_token=Bearer_xyz123456789"],
        [detector, "auth_token=jwt.payload.signature123", "auth_token=jwt.payload.signature123"],
        [detector, "api-key=prod-key-abc123xyz456", "api-key=prod-key-abc123xyz456"],
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        [detector, "sk-short"],
        [detector, "ghp_tooshort"],
        [detector, "api_key="],
        [detector, "token=abc"],
        [detector, "just some plain text without keys"],
        [detector, "12345678901234567890"],
        [detector, "secret="],
        [detector, "no-prefix-key-here"],
    ];
}
