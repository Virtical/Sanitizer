using Sanitizer.Service.Detectors;

namespace Sanitizer.Test;

public class TestBase
{
    protected EmailDetector emailDetector = new();
    protected PhoneDetector phoneDetector = new();
    protected CreditCardDetector cardDetector = new();
    protected GuidDetector guidDetector = new();
    protected ApiKeyDetector apiKeyDetector = new();
    protected IpAddressDetector ipAddressDetector = new();
}