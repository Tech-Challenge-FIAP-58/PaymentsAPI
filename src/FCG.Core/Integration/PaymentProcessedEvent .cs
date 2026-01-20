namespace FCG.Core.Integration;

public class PaymentProcessedEvent
{
    public Guid OrderId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public PaymentResultStatus Status { get; }
    public string? Reason { get; }

    public PaymentProcessedEvent(
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
