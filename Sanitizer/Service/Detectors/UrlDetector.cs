using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Sanitizer.Service.Detectors;

public class UrlDetector : IDetector
{
    public ItemMatch[] Find(string text)
    {
        return SequenceRecognizer
            .RecognizeURL(text, Culture.English)
            .Where(x => x.Text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        x.Text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .Select(x => new ItemMatch { Value = x.Text, Position = x.Start })
            .ToArray();
    }
}
