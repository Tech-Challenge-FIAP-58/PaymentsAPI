using FCG.Payments.Models;

namespace FCG.Payments.Facade
{
    public interface IPaymentFacade
    {
        Task<Transaction> ProcessPayment(Payment payment);
    }
}