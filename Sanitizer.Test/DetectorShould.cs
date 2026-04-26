using FluentAssertions;
using Sanitizer.Service.Detectors;
using Sanitizer.Test.Cases;

namespace Sanitizer.Test;

[TestFixture]
public class DetectorShould
{
    [TestCaseSource(typeof(EmailDetectorCases), nameof(EmailDetectorCases.Valid))]
    [TestCaseSource(typeof(PhoneDetectorCases), nameof(PhoneDetectorCases.Valid))]
    [TestCaseSource(typeof(IpAddressDetectorCases), nameof(IpAddressDetectorCases.Valid))]
    [TestCaseSource(typeof(GuidDetectorCases), nameof(GuidDetectorCases.Valid))]
    [TestCaseSource(typeof(UrlDetectorCases), nameof(UrlDetectorCases.Valid))]
    [TestCaseSource(typeof(ApiKeyDetectorCases), nameof(ApiKeyDetectorCases.Valid))]
    [TestCaseSource(typeof(CartDetectorCases), nameof(CartDetectorCases.Valid))]
    [TestCaseSource(typeof(NameDetectorCases), nameof(NameDetectorCases.Valid))]
    public void Detect(IDetector detector, string text, string expected)
    {
        var actuals = detector.Find(text);
        
        actuals.Should().HaveCount(1);
        actuals.First().Value.Should().Be(expected);
    }
    [TestCaseSource(typeof(EmailDetectorCases), nameof(EmailDetectorCases.Invalid))]
    [TestCaseSource(typeof(PhoneDetectorCases), nameof(PhoneDetectorCases.Invalid))]
    [TestCaseSource(typeof(IpAddressDetectorCases), nameof(IpAddressDetectorCases.Invalid))]
    [TestCaseSource(typeof(GuidDetectorCases), nameof(GuidDetectorCases.Invalid))]
    [TestCaseSource(typeof(UrlDetectorCases), nameof(UrlDetectorCases.Invalid))]
    [TestCaseSource(typeof(ApiKeyDetectorCases), nameof(ApiKeyDetectorCases.Invalid))]
    [TestCaseSource(typeof(CartDetectorCases), nameof(CartDetectorCases.Invalid))]
    [TestCaseSource(typeof(NameDetectorCases), nameof(NameDetectorCases.Invalid))]
    public void NotDetect(IDetector detector, string text)
    {
        var actuals = detector.Find(text);
        
        actuals.Should().BeEmpty();
    }
}
