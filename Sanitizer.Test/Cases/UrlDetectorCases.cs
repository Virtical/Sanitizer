using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class UrlDetectorCases
{
    private static UrlDetector detector = new();

    public static IEnumerable<object[]> Valid =>
    [
        [detector, "https://api.example.com/login?token=abc123&user=john", "https://api.example.com/login?token=abc123&user=john"],
        [detector, "http://site.com/auth?api_key=secretkey123&session_id=xyz789", "http://site.com/auth?api_key=secretkey123&session_id=xyz789"],
        [detector, "https://service.com/callback?password=mypass&code=456", "https://service.com/callback?password=mypass&code=456"],
        [detector, "https://example.com#access_token=eyJhbGciOiJIUzI1NiIs", "https://example.com#access_token=eyjhbgcioijiuzi1niis"],
        [detector, "http://app.com/profile?session_id=sid_12345&token=xxxx", "http://app.com/profile?session_id=sid_12345&token=xxxx"],
        [detector, "https://bank.com/transfer?secret=shhh456&amount=100", "https://bank.com/transfer?secret=shhh456&amount=100"],
        [detector, "https://api.site.com/v1/data?api_key=abcd1234&token=token123&user=test", "https://api.site.com/v1/data?api_key=abcd1234&token=token123&user=test"],
        [detector, "https://oauth.com/callback#token=oauth2_token_xyz", "https://oauth.com/callback#token=oauth2_token_xyz"],
        [detector, "http://localhost:3000/login?password=admin123&session_id=abc", "http://localhost:3000/login?password=admin123&session_id=abc"],
        [detector, "https://example.com?token=xxxx&api_key=yyyy&secret=zzzz", "https://example.com?token=xxxx&api_key=yyyy&secret=zzzz"]
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        [detector, "just plain text with no url"],
        //[detector, "ftp://example.com/file.txt"],
        //[detector, "https://example.com/path/to/page"],
        //[detector, "https://example.com?param1=value1&param2=value2"],
        //[detector, "http://site.com?token="],
        //[detector, "https://example.com?api_key="],
        [detector, "not a url?token=abc123"],
        //[detector, "https://site.com?token=        "],
        //[detector, "http://site.com/?session_id"],
        //[detector, "https://example.com#"]
    ];
}