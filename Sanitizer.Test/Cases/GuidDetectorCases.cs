using Sanitizer.Service.Detectors;

namespace Sanitizer.Test.Cases;

public class GuidDetectorCases
{
    private static GuidDetector detector = new();

    public static IEnumerable<object[]> Valid =>
    [
        [detector, "550e8400-e29b-41d4-a716-446655440000", "550e8400-e29b-41d4-a716-446655440000"],
        [detector, "550e8400e29b41d4a716446655440000", "550e8400e29b41d4a716446655440000"],
        [detector, "{550e8400-e29b-41d4-a716-446655440000}", "{550e8400-e29b-41d4-a716-446655440000}"],
        [detector, "123e4567-e89b-12d3-a456-426614174000", "123e4567-e89b-12d3-a456-426614174000"],
        [detector, "f47ac10b-58cc-4372-a567-0e02b2c3d479", "f47ac10b-58cc-4372-a567-0e02b2c3d479"],
        [detector, "transaction id: 550e8400-e29b-41d4-a716-446655440000", "550e8400-e29b-41d4-a716-446655440000"],
        [detector, "guid {f47ac10b-58cc-4372-a567-0e02b2c3d479} found", "{f47ac10b-58cc-4372-a567-0e02b2c3d479}"],
        [detector, "6ba7b810-9dad-11d1-80b4-00c04fd430c8", "6ba7b810-9dad-11d1-80b4-00c04fd430c8"],
        [detector, "00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000"],
        [detector, "3b1f8b40-3c5f-4f8b-8b8b-8b8b8b8b8b8b", "3b1f8b40-3c5f-4f8b-8b8b-8b8b8b8b8b8b"]
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        [detector, "550e8400-e29b-41d4-a716-44665544000"],
        [detector, "550e8400e29b41d4a71644665544000"],
        [detector, "550e8400-e29b-41d4-a716-4466554400000"],
        [detector, "123e4567-e89b-12d3-a456-42661417400"],
        [detector, "550e8400-z29b-41d4-a716-446655440000"],
        [detector, "550e8400-e29b-41d4-a716"],
        [detector, "order-550e8400-e29b-41d4"],
        [detector, "550e8400e29b41d4a7164466554400001"],
        [detector, "not-a-uuid-at-all"],
    ];
}