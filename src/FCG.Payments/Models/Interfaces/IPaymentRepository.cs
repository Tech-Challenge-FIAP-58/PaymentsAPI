namespace FCG.Payments.Models.Interfaces
{
    public interface IPaymentRepository
    {
        void AddPayment(Payment payment);
        void AddTransaction(Transaction transaction);
        Task<Payment> GetPaymentByOrderId(int orderId);
        Task<IEnumerable<Transaction>> GetTransactionsByOrderId(int orderId);
    }
}
