using Sanitizer.Api.Models;
using Sanitizer.Service.Detectors;

namespace Sanitizer.Api.Services;

/// <summary>Маппинг DetectorType → IDetector.</summary>
public class DetectorRegistry
{
    private readonly Dictionary<DetectorType, IDetector> _detectors = new()
    {
        [DetectorType.Email]     = new EmailDetector(),
        [DetectorType.Phone]     = new PhoneDetector(),
        [DetectorType.IpAddress] = new IpAddressDetector(),
        [DetectorType.Card]      = new CartDetector(),
        [DetectorType.Guid]      = new GuidDetector(),
        [DetectorType.Url]       = new UrlDetector(),
        [DetectorType.ApiKey]    = new ApiKeyDetector(),
        [DetectorType.Name]      = new NameDetector(),
    };

    public IDetector Get(DetectorType type) => _detectors[type];

    public IReadOnlyDictionary<DetectorType, IDetector> All => _detectors;
}
