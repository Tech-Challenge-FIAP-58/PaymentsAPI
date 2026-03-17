using FCG.Payments.Domain.Entities.Mediatr;

namespace FCG.Payments.Domain.Events;

public class PaymentRefundedDomainEvent : Event
{
    public Guid OrderId { get; }
    public decimal Amount { get; }
    public string? Reason { get; }

    public PaymentRefundedDomainEvent(
        Guid orderId,
        decimal amount,
        string? reason = null)
    {
        OrderId = orderId;
        Amount = amount;
        Reason = reason;
    }
}
