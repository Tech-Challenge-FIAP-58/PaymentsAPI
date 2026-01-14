using FCG.Core.Messages.Integration;
using MassTransit;
using MediatR;

namespace FCG.Payments.Application.Handlers;

public class PaymentProcessedEventHandler
    : INotificationHandler<PaymentProcessedIntegrationEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentProcessedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        PaymentProcessedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(
            new PaymentProcessedIntegrationEvent(
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
