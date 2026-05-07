namespace Sanitizer.Service.Detectors;

public interface IDetector
{
    public ItemMatch[] Find(string text);
}