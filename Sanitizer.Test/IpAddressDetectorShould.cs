using FluentAssertions;

namespace Sanitizer.Test;

[TestFixture]
public class IpAddressDetectorShould : TestBase
{
    [Test]
    [TestCase("192.168.1.1")]
    [TestCase("10.0.0.1")]
    [TestCase("255.255.255.255")]
    public void DetectValidIPv4(string text)
    {
        var result = ipAddressDetector.FindIpAddresses(text);

        result.Should().HaveCount(1);
    }

    [Test]
    [TestCase("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    public void DetectFullIPv6(string text)
    {
        var result = ipAddressDetector.FindIpAddresses(text);

        result.Should().HaveCount(1);
    }

    [Test]
    [TestCase("2001:db8::1")]
    [TestCase("::1")]
    [TestCase("fe80::1ff:fe23:4567:890a")]
    public void DetectShortIPv6(string text)
    {
        var result = ipAddressDetector.FindIpAddresses(text);

        result.Should().HaveCount(1);
    }

    [Test]
    [TestCase("http://192.168.1.1/index.html")]
    [TestCase("Client connected from 10.0.0.1 at time")]
    [TestCase("Error from [2001:db8::1]: timeout")]
    public void DetectIpInContext(string text)
    {
        var result = ipAddressDetector.FindIpAddresses(text);

        result.Should().HaveCount(1);
    }

    [Test]
    [TestCase("999.999.999.999")]
    [TestCase("1.2.3")]
    [TestCase("hello world")]
    [TestCase("300.168.1.1")]
    public void NotDetectInvalidIps(string text)
    {
        var result = ipAddressDetector.FindIpAddresses(text);

        result.Should().BeEmpty();
    }

    [Test]
    public void DetectMultipleIps()
    {
        var text = "Ping 192.168.1.1 and 10.0.0.1";

        var result = ipAddressDetector.FindIpAddresses(text);

        result.Should().HaveCount(2);
    }
}