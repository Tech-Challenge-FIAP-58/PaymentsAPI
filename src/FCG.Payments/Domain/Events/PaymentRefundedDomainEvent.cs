using FCG.Payments.Domain.Entities.Mediatr;

namespace FCG.Payments.Domain.Events;

public class PaymentRefundedDomainEvent : Event
{
    public Guid OrderId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string? Reason { get; }

    public PaymentRefundedDomainEvent(
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
