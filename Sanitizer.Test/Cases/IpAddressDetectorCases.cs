using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class IpAddressDetectorCases : BaseCases<IpAddressDetector>
{
    public static IEnumerable<TestCaseData> Valid()
    {
        yield return new TestCaseData(detector, "192.168.1.1").Returns("192.168.1.1");
        yield return new TestCaseData(detector, "10.0.0.1").Returns("10.0.0.1");
        yield return new TestCaseData(detector, "2001:0db8:85a3:0000:0000:8a2e:0370:7334").Returns("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        yield return new TestCaseData(detector, "2001:db8::1").Returns("2001:db8::1");
        yield return new TestCaseData(detector, "from 192.168.1.1 connected").Returns("192.168.1.1");
        yield return new TestCaseData(detector, "https://172.16.0.5/login").Returns("172.16.0.5");
        yield return new TestCaseData(detector, "log: 8.8.8.8 responded").Returns("8.8.8.8");
        yield return new TestCaseData(detector, "::1").Returns("::1");
        yield return new TestCaseData(detector, "fe80::1ff:fe23:4567:890a").Returns("fe80::1ff:fe23:4567:890a");
        yield return new TestCaseData(detector, "2001:db8:85a3::8a2e:370:7334").Returns("2001:db8:85a3::8a2e:370:7334");
        yield return new TestCaseData(detector, "192.0.2.1").Returns("192.0.2.1");
        yield return new TestCaseData(detector, "10.10.10.10").Returns("10.10.10.10");
    }

    public static IEnumerable<TestCaseData> Invalid()
    {
        yield return new TestCaseData(detector, "999.999.999.999").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "1.2.3").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "256.100.50.25").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "192.168.1").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "2001:gggg::1").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "192.168.1.1.1").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "localhost 192.168").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "300.300.300.300").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "2001:db8::1::2").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "1.1.1").Returns(Array.Empty<string>());
    }
}
