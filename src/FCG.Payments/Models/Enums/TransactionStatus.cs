namespace FCG.Payments.Models.Enums;

public enum TransactionStatus
{
    Authorized = 1,
    Paid,
    Declined,
    Refunded,
    Cancelled
}
