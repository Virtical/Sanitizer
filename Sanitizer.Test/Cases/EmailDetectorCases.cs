using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class EmailDetectorCases : BaseCases<EmailDetector>
{
    public static IEnumerable<TestCaseData> Valid()
    {
        yield return new TestCaseData(detector, "user@example.com").Returns("user@example.com");
        yield return new TestCaseData(detector, "john.doe-work@company.co.uk").Returns("john.doe-work@company.co.uk");
        yield return new TestCaseData(detector, "user_123@domain.com").Returns("user_123@domain.com");
        yield return new TestCaseData(detector, "My email is john.doe@company.com and call me").Returns("john.doe@company.com");
        yield return new TestCaseData(detector, "contact@test.ru, please reply").Returns("contact@test.ru");
        yield return new TestCaseData(detector, "user@россия.рф").Returns("user@россия.рф");
        yield return new TestCaseData(detector, "ivan@пример.рф").Returns("ivan@пример.рф");
        yield return new TestCaseData(detector, "hello@sub.domain.org").Returns("hello@sub.domain.org");
        yield return new TestCaseData(detector, "very.common@example.com").Returns("very.common@example.com");
        yield return new TestCaseData(detector, "user+tag@domain.com").Returns("user+tag@domain.com");
    }

    public static IEnumerable<TestCaseData> Invalid()
    {
        yield return new TestCaseData(detector, "example@text").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "user@@invalid").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "plainaddress").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "missing@domain").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "@missingusername.com").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "user@.com").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "user@domain..com").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "just text without email").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "user@domain@com").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "email@domain@domain.com").Returns(Array.Empty<string>());
    }
}
