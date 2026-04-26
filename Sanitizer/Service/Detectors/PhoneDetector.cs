using System.Text.RegularExpressions;

namespace Sanitizer.Service.Detectors;

public class PhoneDetector : IDetector
{
    private static readonly Regex PhoneRegex = new(
        @"(?<!\d)" +
        @"(?:" +
            // Российские форматы: +7 или 8, затем 10 цифр с разделителями
            @"(?:\+7|8)[\s\-]?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}" +
            @"|" +
            // Международные форматы: +CC, затем группы цифр через пробел/дефис
            @"\+[1-9]\d{0,2}[\s\-]?\(?\d{2,4}\)?(?:[\s\-]\d{1,8}){1,4}" +
        @")(?!\d)",
        RegexOptions.Compiled);

    public ItemMatch[] Find(string text)
    {
        return PhoneRegex.Matches(text)
            .Select(m => new ItemMatch { Value = m.Value, Position = m.Index })
            .ToArray();
    }
}
