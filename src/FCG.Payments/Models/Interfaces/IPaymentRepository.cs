namespace FCG.Payments.Models.Interfaces
{
    public interface IPaymentRepository
    {
        void AddPayment(Payment payment);
        void AddTransaction(Transaction transaction);
        Task<Payment> GetPaymentByOrderId(Guid orderId);
        Task<IEnumerable<Transaction>> GetTransactionsByOrderId(Guid orderId);
    }
}
