using FCG.Core.Contracts;
using FCG.Payments.Application.Services;
using FCG.Payments.Consumers;
using FluentAssertions;
using MassTransit;
using Moq;

namespace FCG.Payments.Test.Consumers;

public class OrderPlacedEventConsumerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly OrderPlacedEventConsumer _consumer;

    public OrderPlacedEventConsumerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _consumer = new OrderPlacedEventConsumer(_paymentServiceMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldCallPaymentService_WhenMessageIsReceived()
    {
        // Arrange
        var orderPlacedEvent = CreateOrderPlacedEvent();
        var contextMock = new Mock<ConsumeContext<OrderPlacedEvent>>();
        contextMock.Setup(x => x.Message).Returns(orderPlacedEvent);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _paymentServiceMock.Verify(
            x => x.ProcessPayment(It.Is<OrderPlacedEvent>(
                e => e.OrderId == orderPlacedEvent.OrderId
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Consume_ShouldProcessCorrectOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderPlacedEvent = new OrderPlacedEvent(
            1,
            orderId,
            1,
            100m,
            "John Doe",
            "1234567890123456",
            "12/25",
            "123"
        );

        var contextMock = new Mock<ConsumeContext<OrderPlacedEvent>>();
        contextMock.Setup(x => x.Message).Returns(orderPlacedEvent);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _paymentServiceMock.Verify(
            x => x.ProcessPayment(It.Is<OrderPlacedEvent>(e => e.OrderId == orderId)),
            Times.Once
        );
    }

    [Fact]
    public async Task Consume_ShouldPropagateException_WhenPaymentServiceThrows()
    {
        // Arrange
        var orderPlacedEvent = CreateOrderPlacedEvent();
        var contextMock = new Mock<ConsumeContext<OrderPlacedEvent>>();
        contextMock.Setup(x => x.Message).Returns(orderPlacedEvent);

        _paymentServiceMock
            .Setup(x => x.ProcessPayment(It.IsAny<OrderPlacedEvent>()))
            .ThrowsAsync(new Exception("Payment processing failed"));

        // Act
        Func<Task> act = async () => await _consumer.Consume(contextMock.Object);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Payment processing failed");
    }

    private static OrderPlacedEvent CreateOrderPlacedEvent()
    {
        return new OrderPlacedEvent(
            1,
            Guid.NewGuid(),
            1,
            100m,
            "John Doe",
            "1234567890123456",
            "12/25",
            "123"
        );
    }
}
