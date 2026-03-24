using FCG.Core.Integration;
using FCG.Payments.Domain.Events;
using MassTransit;
using MediatR;

namespace FCG.Payments.Application.Handlers;

public class PaymentProcessedEventHandler
    : INotificationHandler<PaymentProcessedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IBus _bus;
    private readonly ILogger<PaymentProcessedEventHandler> _logger;

    public PaymentProcessedEventHandler(
        IPublishEndpoint publishEndpoint,
        IBus bus,
        ILogger<PaymentProcessedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(
        PaymentProcessedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(
            new PaymentProcessedEvent(
                notification.OrderId,
                notification.AggregateId,
                notification.Amount,
                notification.Status,
                notification.Reason
            ),
            cancellationToken
        );

        _logger.LogInformation(
            "PaymentProcessedEvent publicado (Publish) OrderId: {OrderId}",
            notification.OrderId
        );

        var endpoint = await _bus.GetSendEndpoint(new Uri("queue:notification-queue"));

        var mensagem = new NotificationMessage
        {
            Destinatario = "cliente@email.com",
            Assunto = "Pagamento processado",
            Corpo = GerarCorpoMensagem(notification)
        };

        await endpoint.Send(mensagem, cancellationToken);

        _logger.LogInformation(
            "NotificationMessage enviada para notification-queue OrderId: {OrderId}",
            notification.OrderId
        );
    }

    private static string GerarCorpoMensagem(PaymentProcessedDomainEvent notification)
    {
        if (notification.Status == PaymentResultStatus.Approved)
        {
            return $"Seu pagamento do pedido {notification.OrderId} foi aprovado com sucesso. Valor: R$ {notification.Amount}";
        }

        return $"Seu pagamento do pedido {notification.OrderId} foi recusado. Motivo: {notification.Reason}";
    }
}

public class NotificationMessage
{
    public string Destinatario { get; set; }
    public string Assunto { get; set; }
    public string Corpo { get; set; }
}
