namespace FCG.Payments.Models.Enums;

internal enum TransactionStatus
{
    Authorized = 1,
    Paid,
    Declined,
    Refunded,
    Cancelled
}
