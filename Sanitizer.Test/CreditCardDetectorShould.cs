using FluentAssertions;
using Sanitizer.Service.Detectors;

namespace Sanitizer.Test;

public class CreditCardDetectorShould : TestBase
{

    [TestCase("4532015112830366")]
    [TestCase("4916338506082832")]
    [TestCase("4556737586899855")]
    [TestCase("4485043483391781")]
    [TestCase("4539148803436467")]
    public void DetectVisaCards(string cardNumber)
    {
        var cards = cardDetector.FindCreditCards(cardNumber);
        
        cards.Should().HaveCount(1);
    }
    
    [TestCase("5500005555555559")]
    [TestCase("5555555555554444")]
    [TestCase("5105105105105100")]
    [TestCase("5425233430109903")]
    public void DetectMasterCardCards(string cardNumber)
    {
        var cards = cardDetector.FindCreditCards(cardNumber);
        
        cards.Should().HaveCount(1);
    }
    
    [TestCase("2201382000000013")]
    [TestCase("2200000000000053")]
    [TestCase("2200777777777779")]
    [TestCase("2200999999999995")]
    public void DetectMirCards(string cardNumber)
    {
        var cards = cardDetector.FindCreditCards(cardNumber);
        
        cards.Should().HaveCount(1);
    }
    
    [TestCase("1234567890123456")]
    [TestCase("1111111111111111")]
    [TestCase("0000000000000000")]
    [TestCase("2222222222222222")]
    [TestCase("9999999999999999")]
    [TestCase("1234567887654321")]
    public void NotDetectSequentialOrRepeatingNumbers(string cardNumber)
    {
        var cards = cardDetector.FindCreditCards(cardNumber);
        
        cards.Should().BeEmpty();
    }
    
    [TestCase("Оплата картой 4532015112830366 в терминале")]
    [TestCase("Номер: 5500 0055 5555 5559")]
    [TestCase("Карта МИР 2201-3820-0000-0013")]
    [TestCase("Please charge 4539 1488 0343 6467 to my Visa")]
    [TestCase("Payment method: 5425-2334-3010-9903")]
    public void DetectCardsInVariousContexts(string text)
    {
        var cards = cardDetector.FindCreditCards(text);
        cards.Should().HaveCount(1);
    }
}