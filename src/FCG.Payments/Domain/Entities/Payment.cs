using FCG.Core.Contracts;
using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Events;

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
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
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
}
