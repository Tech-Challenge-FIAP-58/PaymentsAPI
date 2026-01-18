using FCG.Payments.Domain.Entities;

namespace FCG.Payments.Facade
{
    public interface IPaymentFacade
    {
        Task<Transaction> ProcessPayment(Payment payment);
    }
}