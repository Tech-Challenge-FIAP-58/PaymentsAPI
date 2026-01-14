using FCG.FakePaymentProvider.Enums;
using Microsoft.Extensions.Options;
using FCG.Payments.Models.Enums;
using FCG.FakePaymentProvider;
using FCG.Payments.Models;

namespace FCG.Payments.Facade
{
    public class CreditCardPaymentFacade : IPaymentFacade
    {
        private readonly PaymentConfig _paymentConfig;

        public CreditCardPaymentFacade(IOptions<PaymentConfig> paymentConfig)
        {
            _paymentConfig = paymentConfig.Value;
        }

        public async Task<Transaction> ProcessPayment(Payment payment)
        {
            var paymentService = new FakePaymentService(
                _paymentConfig.DefaultApiKey,
                _paymentConfig.DefaultEncryptionKey);

            var cardHashGen = new CardHash(paymentService)
            {
                CardNumber = payment.CreditCard.CardNumber,
                CardHolderName = payment.CreditCard.CardName,
                CardExpirationDate = payment.CreditCard.CardExpirationDate,
                CardCvv = payment.CreditCard.CVV
            };

            var cardHash = cardHashGen.Generate();

            var transaction = new TransactionFake(paymentService)
            {
                CardHash = cardHash,
                CardNumber = payment.CreditCard.CardNumber,
                CardHolderName = payment.CreditCard.CardName,
                CardExpirationDate = payment.CreditCard.CardExpirationDate,
                CardCvv = payment.CreditCard.CVV,
                PaymentMethod = PaymentMethodFake.CreditCard,
                Amount = payment.Amount
            };

            var result = await transaction.AuthorizeCardTransaction();

            return ToTransaction(result);
        }

        private static Transaction ToTransaction(TransactionFake transaction)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                Status = (TransactionStatus)transaction.Status,
                TotalAmount = transaction.Amount,
                CardBrand = transaction.CardBrand,
                AuthorizationCode = transaction.AuthorizationCode,
                TransactionCost = transaction.Cost,
                TransactionDate = transaction.TransactionDate,
                Nsu = transaction.Nsu,
                Tid = transaction.Tid
            };
        }
    }
}