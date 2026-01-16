using FCG.Core.Messages;
using FCG.Core.Messages.Integration;

namespace FCG.Payments.Domain.Contracts;

public class PaymentProcessedDomainEvent : Event
{
    public int OrderId { get; }
    public int PaymentId { get; }
    public decimal Amount { get; }
    public PaymentResultStatus Status { get; }
    public string? Reason { get; }

    public PaymentProcessedDomainEvent(
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
