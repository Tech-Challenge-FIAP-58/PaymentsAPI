using FCG.Core.Integration;
using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Events;

namespace FCG.Payments.Domain.Entities;

public class Payment : Entity
{
    public Guid OrderId { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public decimal Amount { get; private set; }
    public CreditCard CreditCard { get; private set; }
    public PaymentStatus Status { get; private set; }

    // EF Relation
    public ICollection<Transaction> Transactions { get; set; }

    protected Payment()
    {
        Transactions = new List<Transaction>();
    }

    public static Payment Create(
        Guid orderId,
        PaymentMethod paymentMethod,
        decimal amount,
        CreditCard creditCard)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            PaymentMethod = paymentMethod,
            Amount = amount,
            CreditCard = creditCard,
            Status = PaymentStatus.Pending
        };

        payment.AddEvent(new PaymentCreatedDomainEvent(orderId, amount, paymentMethod));
        return payment;
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);

        if (transaction.Status != TransactionStatus.Authorized)
            AddEvent(new PaymentAttemptFailedDomainEvent(
                transaction.Id, transaction.Status));
    }

    public void Process(Transaction transaction)
    {
        Transactions.Add(transaction);

        var status = transaction.Status == TransactionStatus.Authorized
            ? PaymentResultStatus.Approved
            : PaymentResultStatus.Denied;

        Status = status == PaymentResultStatus.Approved
            ? PaymentStatus.Approved
            : PaymentStatus.Denied;

        AddEvent(new PaymentProcessedDomainEvent(
            OrderId,
            Amount,
            status,
            status == PaymentResultStatus.Denied
                ? "Payment denied by gateway"
                : null
        ));
    }

    public void Refund(string? reason = null)
    {
        if (Status != PaymentStatus.Approved)
            throw new DomainException("Only approved payments can be refunded.");

        Status = PaymentStatus.Refunded;

        AddEvent(new PaymentRefundedDomainEvent(OrderId, Amount, reason));
    }
}
