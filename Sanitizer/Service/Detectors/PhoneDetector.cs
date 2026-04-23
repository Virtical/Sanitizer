using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace Sanitizer.Service.Detectors;

public class PhoneDetector
{
    private readonly Regex phoneRegex;

    public PhoneDetector()
    {
        const string pattern = @"
            (?<=^|\s|[.,:;!?()\[\]{}""']|\n|\r|\t|>)
            (?:
                \+                # Международный префикс
                [1-9]\d{0,2}     # Код страны (1-3 цифры, не начинается с 0)
                [\s.-]?          # Опциональный разделитель
            )?
            (?:
                \(?               # Опциональная открывающая скобка
                \d{1,4}           # Код города/оператора (1-4 цифры)
                \)?               # Опциональная закрывающая скобка
                [\s.-]?           # Опциональный разделитель
            )?
            \d{1,4}               # Первая часть номера (1-4 цифры)
            (?:[\s.-]?\d{1,4})?   # Вторая часть номера (опционально)
            (?:[\s.-]?\d{1,4})?   # Третья часть номера (опционально)
            (?:[\s.-]?\d{1,4})?   # Четвёртая часть номера (опционально)
            (?=\s|$|[.,:;!?()\[\]{}<>""']|\n|\r|\t|<)
        ";

        phoneRegex = new Regex(pattern,
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);
    }

    public IReadOnlyCollection<PhoneMatch> FindPhones(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var matches = new List<PhoneMatch>();

        foreach (Match match in phoneRegex.Matches(text))
        {
            var phone = match.Value;
            
            if (IsValidPhone(phone))
            {
                matches.Add(new PhoneMatch 
                { 
                    Phone = phone, 
                    Position = match.Index 
                });
            }
        }

        return new ReadOnlyCollection<PhoneMatch>(matches);
    }

    private bool IsValidPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return false;

        // Извлекаем только цифры для проверки
        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

        // Телефонный номер должен содержать от 7 до 15 цифр (международный стандарт E.164)
        if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
            return false;

        // Проверка на типичные "похожие, но не телефонные" последовательности
        if (IsNonPhoneNumber(phone, digitsOnly))
            return false;

        // Если есть знак +, номер должен начинаться с +
        if (phone.Contains('+') && !phone.TrimStart().StartsWith('+'))
            return false;

        // Проверка на сбалансированность скобок
        var openBrackets = phone.Count(c => c == '(');
        var closeBrackets = phone.Count(c => c == ')');
        if (openBrackets != closeBrackets)
            return false;
        
        if (openBrackets > 1)
            return false;

        // Проверка, что скобки используются корректно
        if (openBrackets == 1)
        {
            var openIndex = phone.IndexOf('(');
            var closeIndex = phone.IndexOf(')');
            if (openIndex > closeIndex || closeIndex - openIndex > 6)
                return false;
        }

        return true;
    }

    private bool IsNonPhoneNumber(string phone, string digitsOnly)
    {
        // Проверка на номера заказов/коды товаров (три группы по 5+ цифр)
        var groups = phone.Split(new[] { ' ', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
        var digitGroups = groups.Where(g => g.All(char.IsDigit) && g.Length >= 5).ToList();
        if (digitGroups.Count >= 3)
            return true;

        // Проверка на GUID-подобные последовательности (много дефисов с цифрами)
        var dashGroups = phone.Split('-');
        if (dashGroups.Length > 3 && dashGroups.All(g => g.All(char.IsDigit)))
            return true;

        // Проверка на последовательности типа 1234567
        if (digitsOnly.Length >= 6 && IsSequential(digitsOnly))
            return true;

        // Проверка на повторяющиеся цифры (для длинных номеров)
        if (digitsOnly.Length >= 10 && HasRepeatingPattern(digitsOnly, 5))
            return true;

        // Проверка на очень короткий номер без кода страны
        if (!phone.StartsWith('+') && !phone.StartsWith('8') && digitsOnly.Length < 10)
            return false;

        return false;
    }

    private bool HasRepeatingPattern(string digits, int minRepeats)
    {
        if (digits.Length < minRepeats)
            return false;

        for (int i = 0; i <= digits.Length - minRepeats; i++)
        {
            var substring = digits.Substring(i, minRepeats);
            if (substring.Distinct().Count() == 1)
                return true;
        }

        return false;
    }

    private bool IsSequential(string digits)
    {
        if (digits.Length < 6)
            return false;

        bool isAscending = true;
        bool isDescending = true;

        for (int i = 1; i < digits.Length; i++)
        {
            int diff = digits[i] - digits[i - 1];
            
            if (diff != 1)
                isAscending = false;
            if (diff != -1)
                isDescending = false;
        }

        return isAscending || isDescending;
    }

    public string NormalizePhone(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return phone;

        var digits = new string(phone.Where(char.IsDigit).ToArray());

        // Приводим к международному формату
        if (digits.Length == 11 && digits.StartsWith("8"))
        {
            digits = "7" + digits.Substring(1);
        }

        if (!digits.StartsWith("+"))
        {
            digits = "+" + digits;
        }

        return digits;
    }
}

public record PhoneMatch
{
    public required string Phone { get; init; }
    public required int Position { get; init; }
}