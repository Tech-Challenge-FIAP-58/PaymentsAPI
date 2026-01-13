namespace FCG.Core.Messages.Integration;

public class PaymentProcessedEvent: IntegrationEvent
{
    public Guid ClientId { get; private set; }

    public Guid OrderId { get; private set; }

    public PaymentProcessedEvent(Guid clientId, Guid orderId)
    {
        ClientId = clientId;
        OrderId = orderId;
    }
}
