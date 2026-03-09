using FCG.Core.Integration;
using FCG.Payments.Domain.Events;
using MassTransit;
using MediatR;

namespace FCG.Payments.Application.Handlers;

public class PaymentRefundedEventHandler
    : INotificationHandler<PaymentRefundedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentRefundedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        PaymentRefundedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(
            new PaymentRefundedEvent(
                notification.OrderId,
                notification.PaymentId,
                notification.Amount,
                notification.Reason
            ),
            cancellationToken
        );
    }
}
