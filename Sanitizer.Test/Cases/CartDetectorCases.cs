using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class CartDetectorCases
{
    private static readonly CartDetector detector = new();

    public static IEnumerable<object[]> Valid =>
    [
        // Visa
        [detector, "4111111111111111", "4111111111111111"],
        [detector, "4532015112830366", "4532015112830366"],
        [detector, "4111 1111 1111 1111", "4111 1111 1111 1111"],
        [detector, "4532-0151-1283-0366", "4532-0151-1283-0366"],
        [detector, "card: 4111111111111111 ok", "4111111111111111"],
        // MasterCard (51–55)
        [detector, "5425233430109903", "5425233430109903"],
        [detector, "5425-2334-3010-9903", "5425-2334-3010-9903"],
        // MasterCard (2221–2720)
        [detector, "2223003122003222", "2223003122003222"],
        // МИР (2200–2204)
        [detector, "2200000000000004", "2200000000000004"],
        [detector, "оплата картой 2200000000000004 прошла", "2200000000000004"],
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        [detector, "1234567890123456"],   // не Visa/MC/МИР-префикс, Luhn не проходит
        [detector, "4111111111111112"],   // Visa-префикс, но Luhn не проходит
        [detector, "0000000000000000"],   // неверный префикс
        [detector, "4999999999999999"],   // Visa-префикс, но Luhn не проходит
        [detector, "5555555555551234"],   // MC-префикс, но Luhn не проходит
        [detector, "9999999999999999"],   // неверный префикс
        [detector, "just text 12345"],
        [detector, "12345"],
    ];
}
