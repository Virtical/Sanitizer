namespace Sanitizer.Api.Detectors;

public interface IDetector
{
    public ItemMatch[] Find(string text);
}