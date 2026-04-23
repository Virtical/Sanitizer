using FluentAssertions;
using Sanitizer.Service.Detectors;

namespace Sanitizer.Test;

[TestFixture]
public class ApiKeyDetectorShould : TestBase
{
    [Test]
    [TestCase("sk-1234567890abcdef1234")]
    [TestCase("sk-ABCDEFGHIJKLMNOPQRSTUVWXYZ123456")]
    public void DetectOpenAiKeys(string text)
    {
        var result = apiKeyDetector.FindKeys(text);

        result.Should().HaveCount(1);
    }

    [Test]
    [TestCase("ghp_1234567890abcdef1234")]
    [TestCase("gho_ABCDEFGHIJKLMNOPQRSTUV")]
    public void DetectGitHubTokens(string text)
    {
        var result = apiKeyDetector.FindKeys(text);

        result.Should().HaveCount(1);
    }

    [Test]
    [TestCase("api_key=1234567890abcdef")]
    [TestCase("token=abcdef1234567890")]
    [TestCase("secret=supersecretvalue123")]
    [TestCase("api_key = ABCDEFGHIJKLMNOP")]
    public void DetectPrefixedKeys(string text)
    {
        var result = apiKeyDetector.FindKeys(text);

        result.Should().HaveCount(1);
    }

    [Test]
    [TestCase("Authorization: Bearer sk-1234567890abcdef1234")]
    [TestCase("token=abcdef1234567890&user=1")]
    [TestCase("key: ghp_1234567890abcdef1234 in config")]
    public void DetectKeysInContext(string text)
    {
        var result = apiKeyDetector.FindKeys(text);

        result.Should().HaveCount(1);
    }

    [Test]
    public void DetectMultipleKeys()
    {
        var text = "sk-1234567890abcdef1234 and ghp_abcdef1234567890abcd";

        var result = apiKeyDetector.FindKeys(text);

        result.Should().HaveCount(2);
    }

    [Test]
    [TestCase("")]
    [TestCase("no secrets here")]
    [TestCase("sk-123")]
    [TestCase("ghp_123")]
    [TestCase("api_key=")]
    public void NotDetectInvalidKeys(string text)
    {
        var result = apiKeyDetector.FindKeys(text);

        result.Should().BeEmpty();
    }

    [Test]
    public void SupportCustomPatterns()
    {
        var custom = new[]
        {
            @"custom_[A-Z0-9]{10}"
        };

        var detector = new ApiKeyDetector(custom);

        var result = detector.FindKeys("custom_ABCDEFGHIJ");

        result.Should().HaveCount(1);
    }
}