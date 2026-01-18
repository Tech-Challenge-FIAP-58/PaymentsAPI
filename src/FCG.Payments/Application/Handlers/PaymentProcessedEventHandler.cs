using FCG.Payments.Domain.Events;
using FCG.Core.Contracts;
using MassTransit;
using MediatR;

namespace FCG.Payments.Application.Handlers;

public class PaymentProcessedEventHandler
    : INotificationHandler<PaymentProcessedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentProcessedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        PaymentProcessedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(
            new PaymentProcessedEvent(
                notification.OrderId,
                notification.PaymentId,
                notification.Amount,
                notification.Status,
                notification.Reason
            ),
            cancellationToken
        );
    }
}
