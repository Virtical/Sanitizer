using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;

namespace Sanitizer.Service.Detectors;

public class IpAddressDetector : IDetector
{
    public ItemMatch[] Find(string text)
    {
        return SequenceRecognizer
            .RecognizeIpAddress(text, Culture.English)
            .Where(x => !IsEmbedded(text, x.Start, x.Text.Length))
            .Select(x => new ItemMatch { Value = x.Text, Position = x.Start })
            .ToArray();
    }

    /// <summary>
    /// Проверяет, не является ли найденный адрес частью более длинной последовательности
    /// (например, 192.168.1.1 внутри 192.168.1.1.1).
    /// </summary>
    private static bool IsEmbedded(string text, int start, int length)
    {
        int end = start + length;
        if (start > 0 && (char.IsDigit(text[start - 1]) || text[start - 1] == '.'))
            return true;
        if (end < text.Length && (char.IsDigit(text[end]) || text[end] == '.'))
            return true;
        return false;
    }
}
