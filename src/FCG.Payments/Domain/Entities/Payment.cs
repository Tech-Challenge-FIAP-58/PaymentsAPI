using FCG.Core.Integration;
using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Events;
using FCG.Payments.Domain;

namespace FCG.Payments.Domain.Entities;

public class Payment : Entity
{
    public Guid OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public CreditCard CreditCard { get; set; }
    public PaymentStatus Status { get; private set; }

    // EF Relation
    public ICollection<Transaction> Transactions { get; set; }

    protected Payment()
    {
        Transactions = new List<Transaction>();
    }

    public Payment(
        Guid orderId,
        PaymentMethod paymentMethod,
        decimal amount,
        CreditCard creditCard)
    {
        OrderId = orderId;
        PaymentMethod = paymentMethod;
        Amount = amount;
        CreditCard = creditCard;
        Transactions = new List<Transaction>();
        Status = PaymentStatus.Pending;

        AddEvent(new PaymentCreatedDomainEvent(orderId, Id, amount, paymentMethod));
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);

        if (transaction.Status != TransactionStatus.Authorized)
            AddEvent(new PaymentAttemptFailedDomainEvent(
                OrderId, Id, transaction.Id, transaction.Status));
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
            Id,
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

        AddEvent(new PaymentRefundedDomainEvent(OrderId, Id, Amount, reason));
    }
}
