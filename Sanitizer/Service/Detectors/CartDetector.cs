using System.Text.RegularExpressions;

namespace Sanitizer.Service.Detectors;

/// <summary>
/// Обнаруживает номера банковских карт (PAN) с валидацией по алгоритму Luhn.
/// Поддерживает Visa, MasterCard, МИР.
/// </summary>
public class CartDetector : IDetector
{
    // 16 цифр с опциональными пробелами или дефисами между группами по 4
    private static readonly Regex CardRegex = new(
        @"(?<!\d)\d{4}[\s\-]?\d{4}[\s\-]?\d{4}[\s\-]?\d{4}(?!\d)",
        RegexOptions.Compiled);

    public ItemMatch[] Find(string text)
    {
        return CardRegex.Matches(text)
            .Where(m => IsValidCard(m.Value))
            .Select(m => new ItemMatch { Value = m.Value, Position = m.Index })
            .ToArray();
    }

    private static bool IsValidCard(string raw)
    {
        var digits = raw.Where(char.IsDigit).ToArray();
        if (digits.Length != 16) return false;

        var number = new string(digits);
        if (!HasKnownPrefix(number)) return false;

        return LuhnCheck(digits);
    }

    private static bool HasKnownPrefix(string number)
    {
        // Visa: начинается с 4
        if (number[0] == '4') return true;

        var first2 = int.Parse(number[..2]);
        var first4 = int.Parse(number[..4]);

        // MasterCard: 51–55 или 2221–2720
        if (first2 is >= 51 and <= 55) return true;
        if (first4 is >= 2221 and <= 2720) return true;

        // МИР: 2200–2204
        if (first4 is >= 2200 and <= 2204) return true;

        return false;
    }

    private static bool LuhnCheck(char[] digits)
    {
        var sum = 0;
        var doubleIt = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var d = digits[i] - '0';
            if (doubleIt) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
            doubleIt = !doubleIt;
        }
        return sum % 10 == 0;
    }
}
