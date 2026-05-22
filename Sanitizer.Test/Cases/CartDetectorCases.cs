using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class CartDetectorCases : BaseCases<CartDetector>
{
    public static IEnumerable<TestCaseData> Valid()
    {
        yield return new TestCaseData(detector, "4111111111111111").Returns("4111111111111111");
        yield return new TestCaseData(detector, "4532015112830366").Returns("4532015112830366");
        yield return new TestCaseData(detector, "4111 1111 1111 1111").Returns("4111 1111 1111 1111");
        yield return new TestCaseData(detector, "4532-0151-1283-0366").Returns("4532-0151-1283-0366");
        yield return new TestCaseData(detector, "card: 4111111111111111 ok").Returns("4111111111111111");
        yield return new TestCaseData(detector, "5425233430109903").Returns("5425233430109903");
        yield return new TestCaseData(detector, "5425-2334-3010-9903").Returns("5425-2334-3010-9903");
        yield return new TestCaseData(detector, "2223003122003222").Returns("2223003122003222");
        yield return new TestCaseData(detector, "2200000000000004").Returns("2200000000000004");
        yield return new TestCaseData(detector, "оплата картой 2200000000000004 прошла").Returns("2200000000000004");
    }

    public static IEnumerable<TestCaseData> Invalid()
    {
        yield return new TestCaseData(detector, "1234567890123456").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "4111111111111112").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "0000000000000000").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "4999999999999999").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "9999999999999999").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "just text 12345").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "12345").Returns(Array.Empty<string>());
    }
}
