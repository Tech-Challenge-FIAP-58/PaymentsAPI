using FCG.Payments.Models.Enums;
using FCG.Core.DomainObjects;

namespace FCG.Payments.Models;

public class Transaction : Entity
{
    public string AuthorizationCode { get; set; }
    public string CardBrand { get; set; } // Ou CardNetwork
    public DateTime? TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TransactionCost { get; set; }
    public TransactionStatus Status { get; set; }
    public string Tid { get; set; } // Transaction Identifier
    public string Nsu { get; set; } // Sequential Network Number (Comum em gateways)

    public int PaymentId { get; set; }

    // EF Relation
    public Payment Payment { get; set; }
}
