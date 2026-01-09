namespace FCG.Payments.Models.Interfaces
{
    internal interface IPaymentRepository
    {
        void AddPayment(Payment payment);
        void AddTransaction(Transaction transaction);
        Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
        Task<Payment?> GetPaymentByOrderId(Guid paymentId);
        Task<Payment?> GetTransactionsByOrderId(Guid paymentId);
    }
}
