using Sanitizer.Api.Detectors;

namespace Sanitizer.Test.Cases;

public class UrlDetectorCases : BaseCases<UrlDetector>
{
    public static IEnumerable<TestCaseData> Valid()
    {
        yield return new TestCaseData(detector, "https://api.example.com/login?token=abc123&user=john").Returns("https://api.example.com/login?token=abc123&user=john");
        yield return new TestCaseData(detector, "http://site.com/auth?api_key=secretkey123&session_id=xyz789").Returns("http://site.com/auth?api_key=secretkey123&session_id=xyz789");
        yield return new TestCaseData(detector, "https://service.com/callback?password=mypass&code=456").Returns("https://service.com/callback?password=mypass&code=456");
        yield return new TestCaseData(detector, "https://example.com#access_token=eyJhbGciOiJIUzI1NiIs").Returns("https://example.com#access_token=eyjhbgcioijiuzi1niis");
        yield return new TestCaseData(detector, "http://app.com/profile?session_id=sid_12345&token=xxxx").Returns("http://app.com/profile?session_id=sid_12345&token=xxxx");
        yield return new TestCaseData(detector, "https://bank.com/transfer?secret=shhh456&amount=100").Returns("https://bank.com/transfer?secret=shhh456&amount=100");
        yield return new TestCaseData(detector, "https://api.site.com/v1/data?api_key=abcd1234&token=token123&user=test").Returns("https://api.site.com/v1/data?api_key=abcd1234&token=token123&user=test");
        yield return new TestCaseData(detector, "https://oauth.com/callback#token=oauth2_token_xyz").Returns("https://oauth.com/callback#token=oauth2_token_xyz");
        yield return new TestCaseData(detector, "http://localhost:3000/login?password=admin123&session_id=abc").Returns("http://localhost:3000/login?password=admin123&session_id=abc");
        yield return new TestCaseData(detector, "https://example.com?token=xxxx&api_key=yyyy&secret=zzzz").Returns("https://example.com?token=xxxx&api_key=yyyy&secret=zzzz");
        yield return new TestCaseData(detector, "https://example.com/path/to/page").Returns("https://example.com/path/to/page");
        yield return new TestCaseData(detector, "https://example.com?param1=value1&param2=value2").Returns("https://example.com?param1=value1&param2=value2");
    }

    public static IEnumerable<TestCaseData> Invalid()
    {
        yield return new TestCaseData(detector, "just plain text with no url").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "ftp://example.com/file.txt").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "not a url?token=abc123").Returns(Array.Empty<string>());
    }
}
