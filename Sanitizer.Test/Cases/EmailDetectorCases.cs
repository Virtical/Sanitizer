using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public static class EmailDetectorCases
{
    private static EmailDetector detector = new();

    public static IEnumerable<object[]> Valid =>
    [
        [detector, "user@example.com", "user@example.com"],
        [detector, "john.doe-work@company.co.uk", "john.doe-work@company.co.uk"],
        [detector, "user_123@domain.com", "user_123@domain.com"],
        [detector, "My email is john.doe@company.com and call me", "john.doe@company.com"],
        [detector, "contact@test.ru, please reply", "contact@test.ru"],
        [detector, "user@россия.рф", "user@россия.рф"],
        [detector, "ivan@пример.рф", "ivan@пример.рф"],
        [detector, "hello@sub.domain.org", "hello@sub.domain.org"],
        [detector, "very.common@example.com", "very.common@example.com"],
        [detector, "user+tag@domain.com", "user+tag@domain.com"]
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        [detector, "example@text"],
        [detector, "user@@invalid"],
        [detector, "plainaddress"],
        [detector, "missing@domain"],
        [detector, "@missingusername.com"],
        [detector, "user@.com"],
        [detector, "user@domain..com"],
        [detector, "just text without email"],
        [detector, "email@domain@domain.com"],
        [detector, "user@domain@com"]
    ];
}
