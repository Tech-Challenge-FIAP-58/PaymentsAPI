using FCG.Payments.Models;

namespace FCG.Payments.Facade
{
    public interface IPaymentFacade
    {
        Task<Transaction> AuthorizePayment(Payment payment);
        Task<Transaction> HandlerPayment(Transaction transaction);
        Task<Transaction> CancelAuthorization(Transaction transaction);
    }
}