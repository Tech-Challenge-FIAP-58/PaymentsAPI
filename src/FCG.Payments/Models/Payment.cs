using FCG.Core.Messages.Integration;
using FCG.Payments.Models.Enums;
using FCG.Core.DomainObjects;

namespace FCG.Payments.Models;

public class Payment : Entity
{
    public int OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }

    public CreditCard CreditCard { get; set; }

    // EF Relation
    public ICollection<Transaction> Transactions { get; set; }

    protected Payment()
    {
        Transactions = new List<Transaction>();
    }

    public Payment(
        int orderId,
        PaymentMethod paymentMethod,
        decimal amount,
        CreditCard creditCard)
    {
        OrderId = orderId;
        PaymentMethod = paymentMethod;
        Amount = amount;
        CreditCard = creditCard;
        Transactions = new List<Transaction>();
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

        AddEvent(new PaymentProcessedEvent(
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
