using FCG.Core.Messages.Integration;
using FCG.Payments.Domain.Extensions;
using FCG.Payments.Models.Enums;
using FluentAssertions;

namespace FCG.Payments.Test.Domain.Extensions;

public class OrderEventExtensionsTests
{
    [Fact]
    public void ToPayment_ShouldConvertOrderPlacedEventToPayment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = 1;
        var orderPlacedEvent = new OrderPlacedEvent(
            customerId,
            orderId,
            (int)PaymentMethod.CreditCard,
            150.50m,
            "John Doe",
            "1234567890123456",
            "12/25",
            "123"
        );

        // Act
        var payment = orderPlacedEvent.ToPayment();

        // Assert
        payment.Should().NotBeNull();
        payment.OrderId.Should().Be(orderId);
        payment.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
        payment.Amount.Should().Be(150.50m);
        payment.CreditCard.Should().NotBeNull();
    }

    [Fact]
    public void ToPayment_ShouldMapCreditCardInformation()
    {
        // Arrange
        var orderPlacedEvent = new OrderPlacedEvent(
            1,
            Guid.NewGuid(),
            (int)PaymentMethod.CreditCard,
            200m,
            "Jane Smith",
            "9876543210987654",
            "06/26",
            "456"
        );

        // Act
        var payment = orderPlacedEvent.ToPayment();

        // Assert
        payment.CreditCard.CardName.Should().Be("Jane Smith");
        payment.CreditCard.CardNumber.Should().Be("9876543210987654");
        payment.CreditCard.CardExpirationDate.Should().Be("06/26");
        payment.CreditCard.CVV.Should().Be("456");
    }

    [Fact]
    public void ToPayment_ShouldCreatePaymentWithPendingStatus()
    {
        // Arrange
        var orderPlacedEvent = new OrderPlacedEvent(
            1,
            Guid.NewGuid(),
            (int)PaymentMethod.CreditCard,
            99.99m,
            "Test User",
            "1111222233334444",
            "12/27",
            "789"
        );

        // Act
        var payment = orderPlacedEvent.ToPayment();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void ToPayment_ShouldCreatePaymentWithEmptyTransactionsList()
    {
        // Arrange
        var orderPlacedEvent = new OrderPlacedEvent(
            1,
            Guid.NewGuid(),
            (int)PaymentMethod.CreditCard,
            99.99m,
            "Test User",
            "1111222233334444",
            "12/27",
            "789"
        );

        // Act
        var payment = orderPlacedEvent.ToPayment();

        // Assert
        payment.Transactions.Should().NotBeNull();
        payment.Transactions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ToPayment_ShouldMapPaymentMethodCorrectly(int paymentMethodValue)
    {
        // Arrange
        var orderPlacedEvent = new OrderPlacedEvent(
            1,
            Guid.NewGuid(),
            paymentMethodValue,
            99.99m,
            "Test User",
            "1111222233334444",
            "12/27",
            "789"
        );

        // Act
        var payment = orderPlacedEvent.ToPayment();

        // Assert
        payment.PaymentMethod.Should().Be((PaymentMethod)paymentMethodValue);
    }
}
