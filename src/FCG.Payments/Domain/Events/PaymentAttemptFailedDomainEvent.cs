using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Entities.Mediatr;

namespace FCG.Payments.Domain.Events;

public class PaymentAttemptFailedDomainEvent : Event
{
    public Guid TransactionId { get; }
    public TransactionStatus TransactionStatus { get; }

    public PaymentAttemptFailedDomainEvent(
        Guid transactionId,
        TransactionStatus transactionStatus)
    {
        TransactionId = transactionId;
        TransactionStatus = transactionStatus;
    }
}
