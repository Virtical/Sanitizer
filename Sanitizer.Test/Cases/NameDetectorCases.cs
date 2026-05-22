using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class NameDetectorCases : BaseCases<NameDetector>
{
    public static IEnumerable<TestCaseData> Valid()
    {
        yield return new TestCaseData(detector, "Иван Петров").Returns("Иван Петров");
        yield return new TestCaseData(detector, "Мария Иванова").Returns("Мария Иванова");
        yield return new TestCaseData(detector, "Сергей Кузнецов").Returns("Сергей Кузнецов");
        yield return new TestCaseData(detector, "Ольга Морозова").Returns("Ольга Морозова");
        yield return new TestCaseData(detector, "Смирнов Пётр Иванович").Returns("Смирнов Пётр Иванович");
        yield return new TestCaseData(detector, "Соколова Елена Сергеевна").Returns("Соколова Елена Сергеевна");
        yield return new TestCaseData(detector, "John Smith").Returns("John Smith");
        yield return new TestCaseData(detector, "Sarah Johnson").Returns("Sarah Johnson");
        yield return new TestCaseData(detector, "Michael Williams").Returns("Michael Williams");
        yield return new TestCaseData(detector, "Contact: John Williams please").Returns("John Williams");
    }

    public static IEnumerable<TestCaseData> Invalid()
    {
        yield return new TestCaseData(detector, "Москва Россия").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "New York").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "Hello World").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "just plain text").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "123 Main Street").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "Первый Квартал").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "Lorem Ipsum").Returns(Array.Empty<string>());
    }
}
