using FluentAssertions;

namespace Sanitizer.Test;

public class GuidDetectorShould : TestBase
{
    [TestCase("550e8400-e29b-41d4-a716-446655440000")]
    [TestCase("123e4567-e89b-12d3-a456-426614174000")]
    [TestCase("c9a3e8f1-2b4d-4a7c-9e1f-3d8a2b6c0e4f")]
    [TestCase("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d")]
    [TestCase("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public void DetectStandardGuidFormat(string text)
    {
        var guids = guidDetector.FindGuids(text);
        
        guids.Should().HaveCount(1);
    }

    [TestCase("550e8400e29b41d4a716446655440000")]
    [TestCase("123e4567e89b12d3a456426614174000")]
    [TestCase("c9a3e8f12b4d4a7c9e1f3d8a2b6c0e4f")]
    [TestCase("a1b2c3d4e5f64a7b8c9d0e1f2a3b4c5d")]
    [TestCase("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")]
    [TestCase("00000000000000000000000000000000")]
    public void DetectGuidWithoutDashes(string text)
    {
        var guids = guidDetector.FindGuids(text);
        
        guids.Should().HaveCount(1);
    }
    
    [TestCase("{550e8400-e29b-41d4-a716-446655440000}")]
    [TestCase("{123e4567-e89b-12d3-a456-426614174000}")]
    [TestCase("{c9a3e8f1-2b4d-4a7c-9e1f-3d8a2b6c0e4f}")]
    [TestCase("{a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d}")]
    [TestCase("{FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF}")]
    [TestCase("{00000000-0000-0000-0000-000000000000}")]
    public void DetectGuidInBraces(string text)
    {
        var guids = guidDetector.FindGuids(text);
        
        guids.Should().HaveCount(1);
    }
    
    [TestCase("Идентификатор: 550e8400-e29b-41d4-a716-446655440000 в системе")]
    [TestCase("Файл {a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d} сохранен")]
    [TestCase("The ID is c9a3e8f12b4d4a7c9e1f3d8a2b6c0e4f in database")]
    [TestCase("Request: 550e8400e29b41d4a716446655440000 status: ok")]
    [TestCase("For GUID {FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF} the result")]
    public void DetectGuidsInVariousContexts(string text)
    {
        var guids = guidDetector.FindGuids(text);
        
        guids.Should().HaveCount(1);
    }
    
    [TestCase("not a guid string")]
    [TestCase("12345678-1234-1234-1234-12345678")]
    [TestCase("GGGGGGGG-GGGG-GGGG-GGGG-GGGGGGGGGGGG")]
    [TestCase("not-a-guid-at-all-just-text")]
    [TestCase("1234567890")]
    public void NotDetectInvalidGuidStrings(string text)
    {
        var guids = guidDetector.FindGuids(text);
        
        guids.Should().BeEmpty();
    }
}