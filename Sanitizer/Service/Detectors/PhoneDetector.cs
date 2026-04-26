using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Sanitizer.Service.Detectors;

public class PhoneDetector : IDetector
{
    public ItemMatch[] Find(string text)
    {
        return SequenceRecognizer
            .RecognizePhoneNumber(text, Culture.English)
            .Select(x => new ItemMatch { Value = x.Text, Position = x.Start } )
            .ToArray();
    }
}