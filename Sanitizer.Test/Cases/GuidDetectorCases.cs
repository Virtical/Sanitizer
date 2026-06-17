using Sanitizer.Api.Detectors;

namespace Sanitizer.Test.Cases;

public class GuidDetectorCases  : BaseCases<GuidDetector>
{
    public static IEnumerable<TestCaseData> Valid()
    {
        yield return new TestCaseData(detector, "550e8400-e29b-41d4-a716-446655440000").Returns("550e8400-e29b-41d4-a716-446655440000");
        yield return new TestCaseData(detector, "550e8400e29b41d4a716446655440000").Returns("550e8400e29b41d4a716446655440000");
        yield return new TestCaseData(detector, "{550e8400-e29b-41d4-a716-446655440000}").Returns("{550e8400-e29b-41d4-a716-446655440000}");
        yield return new TestCaseData(detector, "123e4567-e89b-12d3-a456-426614174000").Returns("123e4567-e89b-12d3-a456-426614174000");
        yield return new TestCaseData(detector, "f47ac10b-58cc-4372-a567-0e02b2c3d479").Returns("f47ac10b-58cc-4372-a567-0e02b2c3d479");
        yield return new TestCaseData(detector, "transaction id: 550e8400-e29b-41d4-a716-446655440000").Returns("550e8400-e29b-41d4-a716-446655440000");
        yield return new TestCaseData(detector, "guid {f47ac10b-58cc-4372-a567-0e02b2c3d479} found").Returns("{f47ac10b-58cc-4372-a567-0e02b2c3d479}");
        yield return new TestCaseData(detector, "6ba7b810-9dad-11d1-80b4-00c04fd430c8").Returns("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
        yield return new TestCaseData(detector, "00000000-0000-0000-0000-000000000000").Returns("00000000-0000-0000-0000-000000000000");
        yield return new TestCaseData(detector, "3b1f8b40-3c5f-4f8b-8b8b-8b8b8b8b8b8b").Returns("3b1f8b40-3c5f-4f8b-8b8b-8b8b8b8b8b8b");
    }

    public static IEnumerable<TestCaseData> Invalid()
    {
        yield return new TestCaseData(detector, "550e8400-e29b-41d4-a716-44665544000").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "550e8400e29b41d4a71644665544000").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "550e8400-e29b-41d4-a716-4466554400000").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "123e4567-e89b-12d3-a456-42661417400").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "550e8400-z29b-41d4-a716-446655440000").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "550e8400-e29b-41d4-a716").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "order-550e8400-e29b-41d4").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "550e8400e29b41d4a7164466554400001").Returns(Array.Empty<string>());
        yield return new TestCaseData(detector, "not-a-uuid-at-all").Returns(Array.Empty<string>());
    }
}