namespace FCG.Core.Messages.Integration;

public class PaymentProcessedEvent : IntegrationEvent
{
    public int OrderId { get; }
    public int PaymentId { get; }
    public decimal Amount { get; }
    public PaymentResultStatus Status { get; }
    public string? Reason { get; }

    public PaymentProcessedEvent(
        int orderId,
        int paymentId,
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
