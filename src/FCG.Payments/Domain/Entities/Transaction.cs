using FCG.Payments.Domain.Entities.Enums;

namespace FCG.Payments.Domain.Entities;

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

    public Guid PaymentId { get; set; }

    // EF Relation
    public Payment Payment { get; set; }
}
