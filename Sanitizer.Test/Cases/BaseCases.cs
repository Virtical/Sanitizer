using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public abstract class BaseCases<TDetector> where TDetector : IDetector, new()
{
    protected static TDetector detector = new();
}