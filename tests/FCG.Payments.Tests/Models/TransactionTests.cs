using FCG.Payments.Domain.Entities;
using FCG.Payments.Domain.Entities.Enums;
using FluentAssertions;

namespace FCG.Payments.Test.Models;

public class TransactionTests
{
    [Fact]
    public void Transaction_ShouldBeCreatedWithAllProperties()
    {
        // Arrange & Act
        var transaction = new Transaction
        {
            AuthorizationCode = "AUTH12345",
            CardBrand = "Visa",
            TransactionDate = DateTime.Now,
            TotalAmount = 150.50m,
            TransactionCost = 3.75m,
            Status = TransactionStatus.Authorized,
            Tid = "TID123456",
            Nsu = "NSU789012",
            PaymentId = Guid.NewGuid()
        };

        // Assert
        transaction.AuthorizationCode.Should().Be("AUTH12345");
        transaction.CardBrand.Should().Be("Visa");
        transaction.TransactionDate.Should().NotBeNull();
        transaction.TotalAmount.Should().Be(150.50m);
        transaction.TransactionCost.Should().Be(3.75m);
        transaction.Status.Should().Be(TransactionStatus.Authorized);
        transaction.Tid.Should().Be("TID123456");
        transaction.Nsu.Should().Be("NSU789012");
        transaction.PaymentId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(TransactionStatus.Authorized)]
    [InlineData(TransactionStatus.Declined)]
    [InlineData(TransactionStatus.Paid)]
    public void Transaction_ShouldSupportDifferentStatuses(TransactionStatus status)
    {
        // Arrange & Act
        var transaction = new Transaction
        {
            Status = status
        };

        // Assert
        transaction.Status.Should().Be(status);
    }

    [Fact]
    public void Transaction_ShouldHaveId_AfterCreation()
    {
        // Arrange & Act
        var transaction = new Transaction();

        // Assert
        transaction.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("Visa")]
    [InlineData("Mastercard")]
    [InlineData("American Express")]
    [InlineData("Elo")]
    public void Transaction_ShouldAcceptDifferentCardBrands(string cardBrand)
    {
        // Arrange & Act
        var transaction = new Transaction
        {
            CardBrand = cardBrand
        };

        // Assert
        transaction.CardBrand.Should().Be(cardBrand);
    }

    [Fact]
    public void Transaction_ShouldAllowNullTransactionDate()
    {
        // Arrange & Act
        var transaction = new Transaction
        {
            TransactionDate = null
        };

        // Assert
        transaction.TransactionDate.Should().BeNull();
    }
}
