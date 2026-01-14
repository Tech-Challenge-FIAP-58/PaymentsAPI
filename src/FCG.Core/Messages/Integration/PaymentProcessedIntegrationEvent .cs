namespace FCG.Core.Messages.Integration;

public class PaymentProcessedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public PaymentResultStatus Status { get; }
    public string? Reason { get; }

    public PaymentProcessedIntegrationEvent(
        Guid orderId,
        Guid paymentId,
        decimal amount,
        PaymentResultStatus status,
        string? reason = null)
    {
        OrderId = orderId;
        PaymentId = paymentId;
        Amount = amount;
        Status = status;
        Reason = reason;
    }
}

public enum PaymentResultStatus
{
    Approved = 1,
    Denied = 2
}
