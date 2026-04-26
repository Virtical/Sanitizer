using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class PhoneDetectorCases
{
    private static PhoneDetector detector = new();

    public static IEnumerable<object[]> Valid =>
    [
        [detector, "+7 (999) 123-45-67", "+7 (999) 123-45-67"],
        [detector, "89991234567", "89991234567"],
        [detector, "+7 999 123 45 67", "+7 999 123 45 67"],
        [detector, "+1-555-123-4567", "+1-555-123-4567"],
        [detector, "+44 20 7946 0958", "+44 20 7946 0958"],
        [detector, "8-999-123-45-67", "8-999-123-45-67"],
        [detector, "+7 916 222 33 44", "+7 916 222 33 44"],
        [detector, "8(495)123-45-67", "8(495)123-45-67"],
        [detector, "+49 30 12345678", "+49 30 12345678"],
        [detector, "+359 88 888 8888", "+359 88 888 8888"],
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        [detector, "12345"],
        //[detector, "заказ № 89991234567"],   // контекстная проверка — вне возможностей regex
        //[detector, "код товара 1234567890"],  // контекстная проверка — вне возможностей regex
        [detector, "999 123"],
        [detector, "abc+7def"],
        [detector, "номер карты 5555555555554444"],
        [detector, "123-45-67"],
        [detector, "8 800 123"],
    ];
}
