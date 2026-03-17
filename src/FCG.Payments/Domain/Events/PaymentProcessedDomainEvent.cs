using FCG.Core.Integration;
using FCG.Payments.Domain.Entities.Mediatr;

namespace FCG.Payments.Domain.Events;

public class PaymentProcessedDomainEvent : Event
{
    public Guid OrderId { get; }
    public decimal Amount { get; }
    public PaymentResultStatus Status { get; }
    public string? Reason { get; }

    public PaymentProcessedDomainEvent(
        Guid orderId,
        decimal amount,
        PaymentResultStatus status,
        string? reason = null)
    {
        OrderId = orderId;
        Amount = amount;
        Status = status;
        Reason = reason;
    }
}
