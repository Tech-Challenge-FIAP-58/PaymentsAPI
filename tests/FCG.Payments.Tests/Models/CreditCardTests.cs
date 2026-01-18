using FCG.Payments.Models;
using FluentAssertions;

namespace FCG.Payments.Test.Models;

public class CreditCardTests
{
    [Fact]
    public void Constructor_ShouldCreateCreditCardWithAllProperties()
    {
        // Arrange
        var cardName = "John Doe";
        var cardNumber = "4111111111111111";
        var expirationDate = "12/25";
        var cvv = "123";

        // Act
        var creditCard = new CreditCard(cardName, cardNumber, expirationDate, cvv);

        // Assert
        creditCard.CardName.Should().Be(cardName);
        creditCard.CardNumber.Should().Be(cardNumber);
        creditCard.CardExpirationDate.Should().Be(expirationDate);
        creditCard.CVV.Should().Be(cvv);
    }

    [Fact]
    public void CreditCard_ShouldHaveId_WhenCreated()
    {
        // Arrange & Act
        var creditCard = new CreditCard("John Doe", "4111111111111111", "12/25", "123");

        // Assert
        creditCard.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("Jane Smith", "5555555555554444", "06/26", "456")]
    [InlineData("Bob Johnson", "378282246310005", "03/27", "7890")]
    public void Constructor_ShouldAcceptDifferentCardDetails(
        string cardName,
        string cardNumber,
        string expirationDate,
        string cvv)
    {
        // Act
        var creditCard = new CreditCard(cardName, cardNumber, expirationDate, cvv);

        // Assert
        creditCard.CardName.Should().Be(cardName);
        creditCard.CardNumber.Should().Be(cardNumber);
        creditCard.CardExpirationDate.Should().Be(expirationDate);
        creditCard.CVV.Should().Be(cvv);
    }
}
