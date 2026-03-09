using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Entities.Mediatr;

namespace FCG.Payments.Domain.Events;

public class PaymentAttemptFailedDomainEvent : Event
{
    public Guid OrderId { get; }
    public Guid PaymentId { get; }
    public Guid TransactionId { get; }
    public TransactionStatus TransactionStatus { get; }

    public PaymentAttemptFailedDomainEvent(
        Guid orderId,
        Guid paymentId,
        Guid transactionId,
        TransactionStatus transactionStatus)
    {
        OrderId = orderId;
        PaymentId = paymentId;
        TransactionId = transactionId;
        TransactionStatus = transactionStatus;
    }
}
