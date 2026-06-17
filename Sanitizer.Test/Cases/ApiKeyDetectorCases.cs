using Sanitizer.Api.Detectors;

namespace Sanitizer.Test.Cases;

public class ApiKeyDetectorCases : BaseCases<ApiKeyDetector>
{
    public static IEnumerable<TestCaseData> Valid()
    {
        yield return new TestCaseData(detector, "sk-abcdefghijklmnopqrstuvwxyz123456").Returns("sk-abcdefghijklmnopqrstuvwxyz123456");
        yield return new TestCaseData(detector, "key is sk-ABCDEFGHIJKLMNOPQRSTU1234567890 here").Returns("sk-ABCDEFGHIJKLMNOPQRSTU1234567890");
        yield return new TestCaseData(detector, "ghp_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcd").Returns("ghp_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcd");
        yield return new TestCaseData(detector, "gho_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmn").Returns("gho_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmn");
        yield return new TestCaseData(detector, "api_key=mysecretapikey123").Returns("api_key=mysecretapikey123");
        yield return new TestCaseData(detector, "token=secrettoken789secure").Returns("token=secrettoken789secure");
        yield return new TestCaseData(detector, "secret=mysupersecretvalue1").Returns("secret=mysupersecretvalue1");
        yield return new TestCaseData(detector, "access_token=Bearer_xyz123456789").Returns("access_token=Bearer_xyz123456789");
        yield return new TestCaseData(detector, "auth_token=jwt.payload.signature123").Returns("auth_token=jwt.payload.signature123");
        yield return new TestCaseData(detector, "api-key=prod-key-abc123xyz456").Returns("api-key=prod-key-abc123xyz456");
    }

    public static IEnumerable<TestCaseData> Invalid()
    {
        yield return new TestCaseData(detector, "sk-short").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "ghp_tooshort").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "api_key=").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "token=abc").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "just some plain text without keys").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "12345678901234567890").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "secret=").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "no-prefix-key-here").Returns(Array.Empty<string>());
    }
}
