using FCG.Core.Integration;
using FCG.Payments.Domain.Entities;
using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Events;
using FluentAssertions;

namespace FCG.Payments.Test.Models;

public class PaymentTests
{
    [Fact]
    public void Constructor_ShouldCreatePaymentWithPendingStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var creditCard = CreateCreditCard();

        // Act
        var payment = new Payment(
            orderId,
            PaymentMethod.CreditCard,
            100m,
            creditCard
        );

        // Assert
        payment.OrderId.Should().Be(orderId);
        payment.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
        payment.Amount.Should().Be(100m);
        payment.CreditCard.Should().Be(creditCard);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void AddTransaction_ShouldAddTransactionToList()
    {
        // Arrange
        var payment = CreatePayment();
        var transaction = CreateTransaction(TransactionStatus.Authorized);

        // Act
        payment.AddTransaction(transaction);

        // Assert
        payment.Transactions.Should().HaveCount(1);
        payment.Transactions.Should().Contain(transaction);
    }

    [Fact]
    public void Process_ShouldSetStatusToApproved_WhenTransactionIsAuthorized()
    {
        // Arrange
        var payment = CreatePayment();
        var transaction = CreateTransaction(TransactionStatus.Authorized);

        // Act
        payment.Process(transaction);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Approved);
        payment.Transactions.Should().Contain(transaction);
    }

    [Fact]
    public void Process_ShouldSetStatusToDenied_WhenTransactionIsDenied()
    {
        // Arrange
        var payment = CreatePayment();
        var transaction = CreateTransaction(TransactionStatus.Declined);

        // Act
        payment.Process(transaction);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Denied);
        payment.Transactions.Should().Contain(transaction);
    }

    [Fact]
    public void Process_ShouldAddPaymentProcessedDomainEvent_WhenTransactionIsAuthorized()
    {
        // Arrange
        var payment = CreatePayment();
        var transaction = CreateTransaction(TransactionStatus.Authorized);

        // Act
        payment.Process(transaction);

        // Assert
        payment.Notificacoes.Should().HaveCount(1);
        var domainEvent = payment.Notificacoes.First() as PaymentProcessedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.OrderId.Should().Be(payment.OrderId);
        domainEvent.PaymentId.Should().Be(payment.Id);
        domainEvent.Amount.Should().Be(payment.Amount);
        domainEvent.Status.Should().Be(PaymentResultStatus.Approved);
        domainEvent.Reason.Should().BeNull();
    }

    [Fact]
    public void Process_ShouldAddPaymentProcessedDomainEventWithReason_WhenTransactionIsDenied()
    {
        // Arrange
        var payment = CreatePayment();
        var transaction = CreateTransaction(TransactionStatus.Declined);

        // Act
        payment.Process(transaction);

        // Assert
        payment.Notificacoes.Should().HaveCount(1);
        var domainEvent = payment.Notificacoes.First() as PaymentProcessedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.Status.Should().Be(PaymentResultStatus.Denied);
        domainEvent.Reason.Should().Be("Payment denied by gateway");
    }

    [Fact]
    public void Process_ShouldAddTransactionBeforeChangingStatus()
    {
        // Arrange
        var payment = CreatePayment();
        var transaction = CreateTransaction(TransactionStatus.Authorized);

        // Act
        payment.Process(transaction);

        // Assert
        payment.Transactions.Should().Contain(transaction);
        payment.Status.Should().Be(PaymentStatus.Approved);
    }

    [Fact]
    public void Payment_ShouldAllowMultipleTransactions()
    {
        // Arrange
        var payment = CreatePayment();
        var transaction1 = CreateTransaction(TransactionStatus.Declined);
        var transaction2 = CreateTransaction(TransactionStatus.Declined);
        var transaction3 = CreateTransaction(TransactionStatus.Authorized);

        // Act
        payment.AddTransaction(transaction1);
        payment.AddTransaction(transaction2);
        payment.Process(transaction3);

        // Assert
        payment.Transactions.Should().HaveCount(3);
        payment.Status.Should().Be(PaymentStatus.Approved);
    }

    private static Payment CreatePayment()
    {
        return new Payment(
            Guid.NewGuid(),
            PaymentMethod.CreditCard,
            100m,
            CreateCreditCard()
        );
    }

    private static Transaction CreateTransaction(TransactionStatus status)
    {
        return new Transaction
        {
            AuthorizationCode = "AUTH123",
            CardBrand = "Visa",
            TransactionDate = DateTime.Now,
            TotalAmount = 100m,
            TransactionCost = 2.5m,
            Status = status,
            Tid = "TID123",
            Nsu = "NSU123"
        };
    }

    private static CreditCard CreateCreditCard()
    {
        return new CreditCard(
            "John Doe",
            "1234567890123456",
            "12/25",
            "123"
        );
    }
}
