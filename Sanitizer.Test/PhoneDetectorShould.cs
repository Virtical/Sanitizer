using FluentAssertions;

namespace Sanitizer.Test;

public class PhoneDetectorShould : TestBase
{
    [TestCase("+7 (999) 123-45-67", "+7 (999) 123-45-67")]
    [TestCase("+7 (495) 123-45-67", "+7 (495) 123-45-67")]
    [TestCase("+7 (812) 123-45-67", "+7 (812) 123-45-67")]
    [TestCase("+7 (383) 123-45-67", "+7 (383) 123-45-67")]
    [TestCase("+7 (900) 111-22-33", "+7 (900) 111-22-33")]
    public void DetectPhonesInRussianFormat(string text, string expectedPhone)
    {
        var phones = phoneDetector.FindPhones(text);
        
        phones.Should().HaveCount(1);
        phones.First().Phone.Should().Be(expectedPhone);
    }
    
    [TestCase("89991234567", "89991234567")]
    [TestCase("84951234567", "84951234567")]
    [TestCase("88005553535", "88005553535")]
    [TestCase("89001234567", "89001234567")]
    [TestCase("89161234567", "89161234567")]
    [TestCase("89261234567", "89261234567")]
    public void DetectPhonesInContinuousFormat(string text, string expectedPhone)
    {
        var phones = phoneDetector.FindPhones(text);
        
        phones.Should().HaveCount(1);
        phones.First().Phone.Should().Be(expectedPhone);
    }
    
    [TestCase("+7 999 123 45 67", "+7 999 123 45 67")]
    [TestCase("+7 495 123 45 67", "+7 495 123 45 67")]
    [TestCase("+7 812 123 45 67", "+7 812 123 45 67")]
    [TestCase("+7 999 111 22 33", "+7 999 111 22 33")]
    [TestCase("+7 383 123 45 67", "+7 383 123 45 67")]
    [TestCase("+7 900 123 45 67", "+7 900 123 45 67")]
    public void DetectPhonesWithSpaces(string text, string expectedPhone)
    {
        var phones = phoneDetector.FindPhones(text);
        
        phones.Should().HaveCount(1);
        phones.First().Phone.Should().Be(expectedPhone);
    }
    
    [TestCase("+1-555-123-4567", "+1-555-123-4567")]
    [TestCase("+44 20 7946 0958", "+44 20 7946 0958")]
    [TestCase("+1 212-555-1234", "+1 212-555-1234")]
    [TestCase("+49-30-1234567", "+49-30-1234567")]
    [TestCase("+33 1 23 45 67 89", "+33 1 23 45 67 89")]
    [TestCase("+86 10 1234 5678", "+86 10 1234 5678")]
    public void DetectInternationalPhones(string text, string expectedPhone)
    {
        var phones = phoneDetector.FindPhones(text);
        
        phones.Should().HaveCount(1);
        phones.First().Phone.Should().Be(expectedPhone);
    }
    
    [TestCase("Заказ №12345678901-1234567")]
    [TestCase("Код товара: 1111111111")]
    [TestCase("Серийный номер: 1234-5678-9012-3456")]
    [TestCase("Номер счета: 12345678901234567890")]
    [TestCase("Индекс: 123456")]
    [TestCase("Цена: 1500.99")]
    [TestCase("Версия: 1.2.3.4")]
    [TestCase("not a phone number")]
    public void NotDetectNonPhoneSequences(string text)
    {
        var phones = phoneDetector.FindPhones(text);
        
        phones.Should().BeEmpty();
    }
}