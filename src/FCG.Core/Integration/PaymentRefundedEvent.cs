namespace FCG.Core.Integration;

public class PaymentRefundedEvent
{
    public Guid OrderId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string? Reason { get; }

    public PaymentRefundedEvent(
        Guid orderId,
        Guid paymentId,
        decimal amount,
        string? reason = null)
    {
        OrderId = orderId;
        PaymentId = paymentId;
        Amount = amount;
        Reason = reason;
    }
}
