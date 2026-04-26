using System.Text.RegularExpressions;

namespace Sanitizer.Service.Detectors;

/// <summary>
/// Обнаруживает имена людей по словарям топ-имён/фамилий для RU и EN.
/// Словари доступны для расширения через публичные свойства.
/// </summary>
public class NameDetector : IDetector
{
    private static readonly Regex ThreeWordRegex = new(
        @"\b(\p{Lu}\p{L}+)\s+(\p{Lu}\p{L}+)\s+(\p{Lu}\p{L}+)\b",
        RegexOptions.Compiled);

    private static readonly Regex TwoWordRegex = new(
        @"\b(\p{Lu}\p{L}+)\s+(\p{Lu}\p{L}+)\b",
        RegexOptions.Compiled);

    public static HashSet<string> RuFirstNames { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Александр", "Алексей", "Андрей", "Антон", "Аркадий", "Борис", "Вадим", "Василий",
        "Виктор", "Виталий", "Владимир", "Вячеслав", "Геннадий", "Георгий", "Григорий",
        "Дмитрий", "Евгений", "Иван", "Игорь", "Илья", "Кирилл", "Константин",
        "Леонид", "Максим", "Михаил", "Никита", "Николай", "Олег", "Павел", "Пётр",
        "Роман", "Сергей", "Степан", "Тимур", "Фёдор", "Филипп", "Юрий", "Яков",
        "Александра", "Алина", "Алиса", "Анастасия", "Анна", "Валентина", "Валерия",
        "Вера", "Виктория", "Галина", "Дарья", "Диана", "Екатерина", "Елена", "Ирина",
        "Карина", "Кристина", "Ксения", "Лариса", "Людмила", "Маргарита", "Марина",
        "Мария", "Надежда", "Наталья", "Нина", "Оксана", "Ольга", "Светлана",
        "София", "Татьяна", "Юлия",
    };

    public static HashSet<string> RuLastNames { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Иванов", "Иванова", "Петров", "Петрова", "Сидоров", "Сидорова",
        "Смирнов", "Смирнова", "Кузнецов", "Кузнецова", "Попов", "Попова",
        "Васильев", "Васильева", "Соколов", "Соколова", "Михайлов", "Михайлова",
        "Новиков", "Новикова", "Фёдоров", "Фёдорова", "Морозов", "Морозова",
        "Волков", "Волкова", "Алексеев", "Алексеева", "Лебедев", "Лебедева",
        "Семёнов", "Семёнова", "Козлов", "Козлова", "Степанов", "Степанова",
        "Николаев", "Николаева", "Орлов", "Орлова", "Андреев", "Андреева",
        "Макаров", "Макарова", "Захаров", "Захарова", "Борисов", "Борисова",
        "Романов", "Романова", "Сергеев", "Сергеева", "Дмитриев", "Дмитриева",
        "Королёв", "Королёва", "Гусев", "Гусева",
    };

    public static HashSet<string> RuPatronymics { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Александрович", "Алексеевич", "Андреевич", "Антонович", "Борисович",
        "Васильевич", "Викторович", "Владимирович", "Григорьевич", "Дмитриевич",
        "Евгеньевич", "Иванович", "Игоревич", "Ильич", "Константинович",
        "Максимович", "Михайлович", "Николаевич", "Павлович", "Петрович",
        "Романович", "Сергеевич", "Степанович", "Юрьевич",
        "Александровна", "Алексеевна", "Андреевна", "Антоновна", "Борисовна",
        "Васильевна", "Викторовна", "Владимировна", "Григорьевна", "Дмитриевна",
        "Евгеньевна", "Ивановна", "Игоревна", "Константиновна", "Максимовна",
        "Михайловна", "Николаевна", "Павловна", "Петровна", "Романовна",
        "Сергеевна", "Степановна", "Юрьевна",
    };

    public static HashSet<string> EnFirstNames { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph",
        "Thomas", "Charles", "Christopher", "Daniel", "Matthew", "Anthony", "Mark",
        "Donald", "Steven", "Paul", "Andrew", "Kenneth", "George", "Joshua", "Kevin",
        "Brian", "Timothy", "Ronald", "Edward", "Jason", "Jeffrey", "Ryan", "Gary",
        "Mary", "Patricia", "Jennifer", "Linda", "Barbara", "Elizabeth", "Susan",
        "Jessica", "Sarah", "Karen", "Lisa", "Nancy", "Betty", "Sandra", "Ashley",
        "Emily", "Donna", "Michelle", "Amanda", "Melissa", "Rebecca", "Sharon",
        "Laura", "Cynthia", "Anna", "Dorothy", "Carol",
    };

    public static HashSet<string> EnLastNames { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
        "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White",
        "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker",
        "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill",
        "Flores", "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell",
        "Mitchell", "Carter", "Roberts", "Turner", "Phillips", "Evans", "Collins",
        "Stewart", "Morris", "Reed", "Cook", "Morgan", "Bell", "Murphy", "Bailey",
        "Cooper", "Richardson", "Cox", "Howard", "Ward", "Brooks", "Watson",
        "Kelly", "Sanders", "Price", "Bennett", "Wood", "Barnes", "Ross",
        "Henderson", "Coleman", "Jenkins", "Perry", "Powell", "Long", "Patterson",
        "Hughes", "Washington", "Butler", "Simmons", "Foster", "Bryant",
        "Alexander", "Russell", "Griffin", "Diaz", "Hayes",
    };

    public ItemMatch[] Find(string text)
    {
        var results = new List<ItemMatch>();
        var usedRanges = new List<(int Start, int End)>();

        // Первый проход: ФИО из трёх слов (Фамилия Имя Отчество)
        foreach (Match m in ThreeWordRegex.Matches(text))
        {
            var w1 = m.Groups[1].Value;
            var w2 = m.Groups[2].Value;
            var w3 = m.Groups[3].Value;

            if (IsFio(w1, w2, w3))
            {
                results.Add(new ItemMatch { Value = m.Value, Position = m.Index });
                usedRanges.Add((m.Index, m.Index + m.Length));
            }
        }

        // Второй проход: пары Имя+Фамилия (без пересечений с ФИО)
        foreach (Match m in TwoWordRegex.Matches(text))
        {
            if (usedRanges.Any(r => m.Index >= r.Start && m.Index < r.End))
                continue;

            var w1 = m.Groups[1].Value;
            var w2 = m.Groups[2].Value;

            if (IsNameSurnamePair(w1, w2))
                results.Add(new ItemMatch { Value = m.Value, Position = m.Index });
        }

        return results.OrderBy(r => r.Position).ToArray();
    }

    private static bool IsFio(string w1, string w2, string w3) =>
        RuPatronymics.Contains(w3) && IsNameSurnamePair(w1, w2);

    private static bool IsNameSurnamePair(string a, string b) =>
        (RuFirstNames.Contains(a) && RuLastNames.Contains(b)) ||
        (RuLastNames.Contains(a) && RuFirstNames.Contains(b)) ||
        (EnFirstNames.Contains(a) && EnLastNames.Contains(b))  ||
        (EnLastNames.Contains(a) && EnFirstNames.Contains(b));
}
