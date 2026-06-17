using Sanitizer.Api.Detectors;
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
    public string Detect(IDetector detector, string text)
        => detector
            .Find(text)
            .First()
            .Value;

    [TestCaseSource(typeof(EmailDetectorCases), nameof(EmailDetectorCases.Invalid))]
    [TestCaseSource(typeof(PhoneDetectorCases), nameof(PhoneDetectorCases.Invalid))]
    [TestCaseSource(typeof(IpAddressDetectorCases), nameof(IpAddressDetectorCases.Invalid))]
    [TestCaseSource(typeof(GuidDetectorCases), nameof(GuidDetectorCases.Invalid))]
    [TestCaseSource(typeof(UrlDetectorCases), nameof(UrlDetectorCases.Invalid))]
    [TestCaseSource(typeof(ApiKeyDetectorCases), nameof(ApiKeyDetectorCases.Invalid))]
    [TestCaseSource(typeof(CartDetectorCases), nameof(CartDetectorCases.Invalid))]
    [TestCaseSource(typeof(NameDetectorCases), nameof(NameDetectorCases.Invalid))]
    public string[] NotDetect(IDetector detector, string text)
        => detector
            .Find(text)
            .Select(x => x.Value)
            .ToArray();
}
