using FCG.Core.Integration;
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
using System.Net;
using System.Text;
using System.Text.Json;

namespace FCG.Payments.Test.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentFacade> _paymentFacadeMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediatorHandler> _mediatorHandlerMock;

    public PaymentServiceTests()
    {
        _paymentFacadeMock = new Mock<IPaymentFacade>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorHandlerMock = new Mock<IMediatorHandler>();
    }

    [Fact]
    public async Task ProcessPayment_ShouldAuthorizeOnFirstAttempt_WhenPaymentIsSuccessful()
    {
        // Arrange
        var paymentProviderHandler = new PaymentProviderHandler(TransactionStatus.Authorized);
        var paymentService = CreatePaymentService(paymentProviderHandler);
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        // Act
        await paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        paymentProviderHandler.CallCount.Should().Be(1);
        _paymentRepositoryMock.Verify(x => x.AddPayment(It.IsAny<Payment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldRetryThreeTimes_WhenPaymentFails()
    {
        // Arrange
        var paymentProviderHandler = new PaymentProviderHandler(
            TransactionStatus.Declined,
            TransactionStatus.Declined,
            TransactionStatus.Declined);
        var paymentService = CreatePaymentService(paymentProviderHandler);
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        // Act
        await paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        paymentProviderHandler.CallCount.Should().Be(3);
        _paymentRepositoryMock.Verify(x => x.AddPayment(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldAuthorizeOnThirdAttempt_WhenPreviousAttemptsFailed()
    {
        // Arrange
        var paymentProviderHandler = new PaymentProviderHandler(
            TransactionStatus.Declined,
            TransactionStatus.Declined,
            TransactionStatus.Authorized);
        var paymentService = CreatePaymentService(paymentProviderHandler);
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        // Act
        await paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        paymentProviderHandler.CallCount.Should().Be(3);
        _paymentRepositoryMock.Verify(x => x.AddPayment(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldNotProcessAgain_WhenOrderAlreadyHasSuccessfulPayment()
    {
        // Arrange
        var paymentProviderHandler = new PaymentProviderHandler(TransactionStatus.Authorized);
        var paymentService = CreatePaymentService(paymentProviderHandler);
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        var existingPayment = Payment.Create(
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
        await paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        paymentProviderHandler.CallCount.Should().Be(0);
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
        var paymentProviderHandler = new PaymentProviderHandler(TransactionStatus.Authorized);
        var paymentService = CreatePaymentService(paymentProviderHandler);
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        // Act
        await paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_ShouldBeginTransaction_BeforeProcessing()
    {
        // Arrange
        var paymentProviderHandler = new PaymentProviderHandler(TransactionStatus.Authorized);
        var paymentService = CreatePaymentService(paymentProviderHandler);
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = CreateOrderPlacedEvent(orderId);

        _paymentRepositoryMock
            .Setup(x => x.GetPaymentByOrderId(orderId))
            .ReturnsAsync(new List<Payment>());

        // Act
        await paymentService.ProcessPayment(orderPlacedEvent);

        // Assert
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private PaymentService CreatePaymentService(PaymentProviderHandler paymentProviderHandler)
    {
        var httpClient = new HttpClient(paymentProviderHandler)
        {
            BaseAddress = new Uri("http://localhost:8000")
        };

        return new PaymentService(
            _paymentFacadeMock.Object,
            _paymentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mediatorHandlerMock.Object,
            httpClient);
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

    private sealed class PaymentProviderHandler : HttpMessageHandler
    {
        private readonly Queue<TransactionStatus> _statuses;

        public int CallCount { get; private set; }

        public PaymentProviderHandler(params TransactionStatus[] statuses)
        {
            _statuses = new Queue<TransactionStatus>(statuses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;

            if (_statuses.Count == 0)
            {
                throw new InvalidOperationException("No mocked transaction status configured.");
            }

            var transaction = CreateTransaction(_statuses.Dequeue());
            var responseBody = JsonSerializer.Serialize(transaction);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
