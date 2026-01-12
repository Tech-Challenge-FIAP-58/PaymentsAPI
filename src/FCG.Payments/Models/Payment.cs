using FCG.Core.DomainObjects;
using FCG.Payments.Models.Enums;

namespace FCG.Payments.Models;

public class Payment : Entity
{
    public Payment()
    {
        Transactions = new List<Transaction>();
    }

    public Guid OrderId { get; set; }
    public PaymentMethod PaymentType { get; set; }
    public decimal Amount { get; set; }

    public CreditCard CreditCard { get; set; }

    // EF Relation
    public ICollection<Transaction> Transactions { get; set; }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction);
    }
}
