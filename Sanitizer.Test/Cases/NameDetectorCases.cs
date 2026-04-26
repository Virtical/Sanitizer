using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class NameDetectorCases
{
    private static readonly NameDetector detector = new();

    public static IEnumerable<object[]> Valid =>
    [
        // RU Имя + Фамилия
        [detector, "Иван Петров", "Иван Петров"],
        [detector, "Мария Иванова", "Мария Иванова"],
        [detector, "Сергей Кузнецов", "Сергей Кузнецов"],
        [detector, "Ольга Морозова", "Ольга Морозова"],
        // RU Фамилия + Имя + Отчество
        [detector, "Смирнов Пётр Иванович", "Смирнов Пётр Иванович"],
        [detector, "Соколова Елена Сергеевна", "Соколова Елена Сергеевна"],
        // EN First + Last
        [detector, "John Smith", "John Smith"],
        [detector, "Sarah Johnson", "Sarah Johnson"],
        [detector, "Michael Williams", "Michael Williams"],
        // Имя в контексте предложения
        [detector, "Клиент Сергей Кузнецов обратился", "Сергей Кузнецов"],
        [detector, "Contact: John Williams please", "John Williams"],
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        [detector, "Москва Россия"],
        [detector, "New York"],
        [detector, "Hello World"],
        [detector, "just plain text"],
        [detector, "123 Main Street"],
        [detector, "Первый Квартал"],
        [detector, "Lorem Ipsum"],
    ];
}
