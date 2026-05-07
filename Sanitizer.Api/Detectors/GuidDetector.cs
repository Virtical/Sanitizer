using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Sanitizer.Service.Detectors;

public class GuidDetector : IDetector
{
    public ItemMatch[] Find(string text)
    {
        return SequenceRecognizer
            .RecognizeGUID(text, Culture.English)
            .Select(x => new ItemMatch { Value = x.Text, Position = x.Start } )
            .ToArray();
    }
}