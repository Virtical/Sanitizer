namespace Sanitizer.Service.Detectors;

using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

public class CreditCardDetector
{
    private readonly Regex cardRegex;

    public CreditCardDetector()
    {
        const string pattern = @"
            (?<=^|\s|[.,:;!?()\[\]{}<>""']|\n|\r|\t)
            \d{4}                    # Первая группа из 4 цифр
            [\s-]?                   # Опциональный разделитель
            \d{4}                    # Вторая группа из 4 цифр
            [\s-]?                   # Опциональный разделитель
            \d{4}                    # Третья группа из 4 цифр
            [\s-]?                   # Опциональный разделитель
            \d{4}                    # Четвёртая группа из 4 цифр
            (?=\s|$|[.,:;!?()\[\]{}<>""']|\n|\r|\t)
        ";

        cardRegex = new Regex(pattern,
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);
    }

    public IReadOnlyCollection<CardMatch> FindCreditCards(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var matches = new List<CardMatch>();

        foreach (Match match in cardRegex.Matches(text))
        {
            var cardNumber = match.Value;
            var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length == 16 && IsValidCardNumber(digitsOnly))
            {
                matches.Add(new CardMatch
                {
                    CardNumber = cardNumber,
                    Position = match.Index
                });
            }
        }

        return new ReadOnlyCollection<CardMatch>(matches);
    }

    private bool IsValidCardNumber(string digits)
    {
        if (digits.Length != 16)
            return false;

        // Проверка, что номер не состоит из одинаковых цифр
        if (digits.Distinct().Count() == 1)
            return false;

        // Проверка, что номер не является последовательностью
        if (IsSequential(digits))
            return false;

        // Проверка алгоритмом Луна
        return IsLuhnValid(digits);
    }

    private bool IsLuhnValid(string digits)
    {
        int sum = 0;
        bool isEven = false;

        // Проходим справа налево
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int digit = digits[i] - '0';

            if (isEven)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            isEven = !isEven;
        }

        return sum % 10 == 0;
    }

    private bool IsSequential(string digits)
    {
        bool ascending = true;
        bool descending = true;

        for (int i = 1; i < digits.Length; i++)
        {
            int diff = digits[i] - digits[i - 1];
            
            if (diff != 1)
                ascending = false;
            if (diff != -1)
                descending = false;
        }

        return ascending || descending;
    }

    public string MaskCardNumber(string cardNumber)
    {
        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());
        
        if (digitsOnly.Length == 16)
        {
            return $"{digitsOnly.Substring(0, 4)} **** **** {digitsOnly.Substring(12, 4)}";
        }

        return cardNumber;
    }
}

public record CardMatch
{
    public required string CardNumber { get; init; }
    public required int Position { get; init; }
}