using Sanitizer.Api.Detectors;

namespace Sanitizer.Test.Cases;

public class PhoneDetectorCases : BaseCases<PhoneDetector>
{
    public static IEnumerable<TestCaseData> Valid()
    {
        yield return new TestCaseData(detector, "+7 (999) 123-45-67").Returns("+7 (999) 123-45-67");
        yield return new TestCaseData(detector, "89991234567").Returns("89991234567");
        yield return new TestCaseData(detector, "+7 999 123 45 67").Returns("+7 999 123 45 67");
        yield return new TestCaseData(detector, "+1-555-123-4567").Returns("+1-555-123-4567");
        yield return new TestCaseData(detector, "+44 20 7946 0958").Returns("+44 20 7946 0958");
        yield return new TestCaseData(detector, "8-999-123-45-67").Returns("8-999-123-45-67");
        yield return new TestCaseData(detector, "+7 916 222 33 44").Returns("+7 916 222 33 44");
        yield return new TestCaseData(detector, "8(495)123-45-67").Returns("8(495)123-45-67");
        yield return new TestCaseData(detector, "+49 30 12345678").Returns("+49 30 12345678");
        yield return new TestCaseData(detector, "+359 88 888 8888").Returns("+359 88 888 8888");
    }

    public static IEnumerable<TestCaseData> Invalid()
    {
        yield return new TestCaseData(detector, "12345").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "999 123").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "abc+7def").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "номер карты 5555555555554444").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "123-45-67").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "8 800 123").Returns(Array.Empty<string>());
    }
}
