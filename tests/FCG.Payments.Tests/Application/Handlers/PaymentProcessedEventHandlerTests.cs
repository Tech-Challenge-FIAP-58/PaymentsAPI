using FCG.Core.Integration;
using FCG.Payments.Application.Handlers;
using FCG.Payments.Domain.Events;
using FluentAssertions;
using MassTransit;
using Moq;

namespace FCG.Payments.Test.Application.Handlers;

public class PaymentProcessedEventHandlerTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly PaymentProcessedEventHandler _handler;

    public PaymentProcessedEventHandlerTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new PaymentProcessedEventHandler(_publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPublishPaymentProcessedEvent()
    {
        // Arrange
        var domainEvent = new PaymentProcessedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            PaymentResultStatus.Approved,
            null
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<PaymentProcessedEvent>(
                    e => e.OrderId == domainEvent.OrderId &&
                         e.PaymentId == domainEvent.PaymentId &&
                         e.Amount == domainEvent.Amount &&
                         e.Status == domainEvent.Status
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldMapDomainEventToIntegrationEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var amount = 150.50m;
        var status = PaymentResultStatus.Approved;

        var domainEvent = new PaymentProcessedDomainEvent(
            orderId,
            paymentId,
            amount,
            status,
            null
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<PaymentProcessedEvent>(
                    e => e.OrderId == orderId &&
                         e.PaymentId == paymentId &&
                         e.Amount == amount &&
                         e.Status == status
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldIncludeReason_WhenPaymentIsDenied()
    {
        // Arrange
        var domainEvent = new PaymentProcessedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            PaymentResultStatus.Denied,
            "Payment denied by gateway"
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<PaymentProcessedEvent>(
                    e => e.Status == PaymentResultStatus.Denied &&
                         e.Reason == "Payment denied by gateway"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var domainEvent = new PaymentProcessedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            PaymentResultStatus.Approved,
            null
        );

        var cancellationToken = new CancellationToken();

        // Act
        await _handler.Handle(domainEvent, cancellationToken);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.IsAny<PaymentProcessedEvent>(),
                cancellationToken
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenPublishFails()
    {
        // Arrange
        var domainEvent = new PaymentProcessedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            PaymentResultStatus.Approved,
            null
        );

        _publishEndpointMock
            .Setup(x => x.Publish(
                It.IsAny<PaymentProcessedEvent>(),
                It.IsAny<CancellationToken>()
            ))
            .ThrowsAsync(new Exception("Failed to publish event"));

        // Act
        Func<Task> act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Failed to publish event");
    }

    [Fact]
    public async Task Handle_Should_Publish_Event_With_Correct_Data()
    {
        // Arrange
        var publishMock = new Mock<IPublishEndpoint>();

        var handler = new PaymentProcessedEventHandler(publishMock.Object);

        var domainEvent = new PaymentProcessedDomainEvent(
            orderId: Guid.NewGuid(),
            paymentId: Guid.NewGuid(),
            amount: 100,
            status: PaymentResultStatus.Approved,
            reason: "OK"
        );

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        publishMock.Verify(x =>
            x.Publish(
                It.Is<PaymentProcessedEvent>(e =>
                    e.OrderId == domainEvent.OrderId &&
                    e.PaymentId == domainEvent.PaymentId &&
                    e.Amount == domainEvent.Amount &&
                    e.Status == domainEvent.Status &&
                    e.Reason == domainEvent.Reason
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

}
