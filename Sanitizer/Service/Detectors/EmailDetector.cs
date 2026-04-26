using System.Text.RegularExpressions;

namespace Sanitizer.Service.Detectors;

public class EmailDetector : IDetector
{
    // Поддерживает стандартные и Unicode/кириллические домены.
    // (?<!@) — исключает случаи вида email@domain@other.com
    private static readonly Regex EmailRegex = new(
        @"(?<!@)[\w+][\w.+\-]*@[\w-]+(\.[\w-]+)*\.\p{L}{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ItemMatch[] Find(string text)
    {
        return EmailRegex.Matches(text)
            .Select(m => new ItemMatch { Value = m.Value, Position = m.Index })
            .ToArray();
    }
}
