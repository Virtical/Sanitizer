using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class IpAddressDetectorCases
{
    private static IpAddressDetector detector = new();

    public static IEnumerable<object[]> Valid =>
    [
        [detector, "192.168.1.1", "192.168.1.1"],
        [detector, "10.0.0.1", "10.0.0.1"],
        [detector, "2001:0db8:85a3:0000:0000:8a2e:0370:7334", "2001:0db8:85a3:0000:0000:8a2e:0370:7334"],
        [detector, "2001:db8::1", "2001:db8::1"],
        [detector, "from 192.168.1.1 connected", "192.168.1.1"],
        [detector, "https://172.16.0.5/login", "172.16.0.5"],
        [detector, "log: 8.8.8.8 responded", "8.8.8.8"],
        [detector, "::1", "::1"],
        [detector, "fe80::1ff:fe23:4567:890a", "fe80::1ff:fe23:4567:890a"],
        [detector, "2001:db8:85a3::8a2e:370:7334", "2001:db8:85a3::8a2e:370:7334"],
        [detector, "192.0.2.1", "192.0.2.1"],
        [detector, "10.10.10.10", "10.10.10.10"]
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        [detector, "999.999.999.999"],
        [detector, "1.2.3"],
        [detector, "256.100.50.25"],
        [detector, "192.168.1"],
        [detector, "2001:gggg::1"],
        [detector, "192.168.1.1.1"],
        [detector, "localhost 192.168"],
        [detector, "300.300.300.300"],
        [detector, "2001:db8::1::2"],
        [detector, "1.1.1"]
    ];
}
