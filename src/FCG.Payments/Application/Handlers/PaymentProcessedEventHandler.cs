using FCG.Core.Messages.Integration;
using MassTransit;
using MediatR;

namespace FCG.Payments.Application.Handlers;

public class PaymentProcessedEventHandler
    : INotificationHandler<PaymentProcessedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentProcessedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        PaymentProcessedEvent notification,
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
