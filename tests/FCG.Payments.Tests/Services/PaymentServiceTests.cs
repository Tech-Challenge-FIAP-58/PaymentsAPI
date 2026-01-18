using FCG.Core.Contracts;
using FCG.Payments.Application.Interfaces;
using FCG.Payments.Application.Mediator;
using FCG.Payments.Application.Services;
using FCG.Payments.Domain.Entities;
using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Entities.Interfaces;
using FCG.Payments.Domain.Events;
using FCG.Payments.Facade;
using FluentAssertions;
using Moq;

namespace FCG.Payments.Test.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentFacade> _paymentFacadeMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediatorHandler> _mediatorHandlerMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _paymentFacadeMock = new Mock<IPaymentFacade>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorHandlerMock = new Mock<IMediatorHandler>();

        _paymentService = new PaymentService(
            _paymentFacadeMock.Object,
            _paymentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mediatorHandlerMock.Object
        );
    }

    [Fact]
    public async Task ProcessPayment_ShouldAuthorizeOnFirstAttempt_WhenPaymentIsSuccessful()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        var authorizedTransaction = CreateTransaction(TransactionStatus.Authorized);

        _paymentFacadeMock
            .Setup(x => x.ProcessPayment(It.IsAny<Payment>()))
            .ReturnsAsync(authorizedTransaction);

        // Act
        await _paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        _paymentFacadeMock.Verify(x => x.ProcessPayment(It.IsAny<Payment>()), Times.Once);
        _paymentRepositoryMock.Verify(x => x.AddPayment(It.IsAny<Payment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldRetryThreeTimes_WhenPaymentFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        var deniedTransaction = CreateTransaction(TransactionStatus.Declined);

        _paymentFacadeMock
            .Setup(x => x.ProcessPayment(It.IsAny<Payment>()))
            .ReturnsAsync(deniedTransaction);

        // Act
        await _paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        _paymentFacadeMock.Verify(x => x.ProcessPayment(It.IsAny<Payment>()), Times.Exactly(3));
        _paymentRepositoryMock.Verify(x => x.AddPayment(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldAuthorizeOnThirdAttempt_WhenPreviousAttemptsFailed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        var deniedTransaction = CreateTransaction(TransactionStatus.Declined);
        var authorizedTransaction = CreateTransaction(TransactionStatus.Authorized);

        _paymentFacadeMock
            .SetupSequence(x => x.ProcessPayment(It.IsAny<Payment>()))
            .ReturnsAsync(deniedTransaction)
            .ReturnsAsync(deniedTransaction)
            .ReturnsAsync(authorizedTransaction);

        // Act
        await _paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        _paymentFacadeMock.Verify(x => x.ProcessPayment(It.IsAny<Payment>()), Times.Exactly(3));
        _paymentRepositoryMock.Verify(x => x.AddPayment(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldNotProcessAgain_WhenOrderAlreadyHasSuccessfulPayment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        var existingPayment = new Payment(
            orderId,
            PaymentMethod.CreditCard,
            100m,
            CreateCreditCard()
        );

        var authorizedTransaction = CreateTransaction(TransactionStatus.Authorized);
        existingPayment.AddTransaction(authorizedTransaction);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment> { existingPayment });

        // Act
        await _paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        _paymentFacadeMock.Verify(x => x.ProcessPayment(It.IsAny<Payment>()), Times.Never);
        _paymentRepositoryMock.Verify(x => x.AddPayment(It.IsAny<Payment>()), Times.Never);
        _mediatorHandlerMock.Verify(
            x => x.PublishEvent(It.Is<PaymentProcessedDomainEvent>(
                e => e.OrderId == orderId && e.Status == PaymentResultStatus.Approved
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessPayment_ShouldPublishDomainEvent_WhenPaymentIsProcessed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        var authorizedTransaction = CreateTransaction(TransactionStatus.Authorized);

        _paymentFacadeMock
            .Setup(x => x.ProcessPayment(It.IsAny<Payment>()))
            .ReturnsAsync(authorizedTransaction);

        // Act
        await _paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldBeginTransaction_BeforeProcessing()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        var authorizedTransaction = CreateTransaction(TransactionStatus.Authorized);

        _paymentFacadeMock
            .Setup(x => x.ProcessPayment(It.IsAny<Payment>()))
            .ReturnsAsync(authorizedTransaction);

        // Act
        await _paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static OrderPlacedEvent CreateOrderPlacedEvent(Guid orderId)
    {
        return new OrderPlacedEvent(
            1,
            orderId,
            1,
            100m,
            "John Doe",
            "1234567890123456",
            "12/25",
            "123"
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
