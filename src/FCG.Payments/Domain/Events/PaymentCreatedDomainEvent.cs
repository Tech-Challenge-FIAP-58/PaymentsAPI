using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Entities.Mediatr;

namespace FCG.Payments.Domain.Events;

public class PaymentCreatedDomainEvent : Event
{
    public Guid OrderId { get; }
    public decimal Amount { get; }
    public PaymentMethod PaymentMethod { get; }

    public PaymentCreatedDomainEvent(
        Guid orderId,
        decimal amount,
        PaymentMethod paymentMethod)
    {
        OrderId = orderId;
        Amount = amount;
        PaymentMethod = paymentMethod;
    }
}
