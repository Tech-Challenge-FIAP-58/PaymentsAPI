namespace FCG.Payments.Domain.Entities.Interfaces
{
    public interface IPaymentRepository
    {
        void AddPayment(Payment payment);
        void AddTransaction(Transaction transaction);
        Task<IEnumerable<Payment>> GetPaymentByOrderId(Guid orderId);
        Task<IEnumerable<Transaction>> GetTransactionsByOrderId(Guid orderId);
    }
}
