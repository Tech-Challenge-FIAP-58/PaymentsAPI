using FCG.FakePaymentProvider;
using FCG.FakePaymentProvider.Enums;
using FCG.Payments.Models;
using FCG.Payments.Models.Enums;
using Microsoft.Extensions.Options;

namespace FCG.Payments.Facade
{
    public class CreditCardPaymentFacade : IPaymentFacade
    {
        private readonly PaymentConfig _paymentConfig;

        public CreditCardPaymentFacade(IOptions<PaymentConfig> paymentConfig)
        {
            _paymentConfig = paymentConfig.Value;
        }

        public async Task<Transaction> AuthorizePayment(Payment payment)
        {
            var nerdsPagSvc = new FakePaymentService(_paymentConfig.DefaultApiKey,
                _paymentConfig.DefaultEncryptionKey);

            var cardHashGen = new CardHash(nerdsPagSvc)
            {
                CardNumber = payment.CreditCard.CardNumber,
                CardHolderName = payment.CreditCard.CardName,
                CardExpirationDate = payment.CreditCard.CardExpirationDate,
                CardCvv = payment.CreditCard.CVV
            };
            var cardHash = cardHashGen.Generate();

            var transaction = new TransactionFake(nerdsPagSvc)
            {
                CardHash = cardHash,
                CardNumber = payment.CreditCard.CardNumber,
                CardHolderName = payment.CreditCard.CardName,
                CardExpirationDate = payment.CreditCard.CardExpirationDate,
                CardCvv = payment.CreditCard.CVV,
                PaymentMethod = PaymentMethodFake.CreditCard,
                Amount = payment.Amount
            };

            return ToTransaction(await transaction.AuthorizeCardTransaction());
        }

        public async Task<Transaction> HandlerPayment(Transaction Transaction)
        {
            var nerdsPagSvc = new FakePaymentService(_paymentConfig.DefaultApiKey,
                _paymentConfig.DefaultEncryptionKey);

            var transaction = ToTransactionFake(Transaction, nerdsPagSvc);

            return ToTransaction(await transaction.CaptureCardTransaction());
        }

        public async Task<Transaction> CancelAuthorization(Transaction Transaction)
        {
            var nerdsPagSvc = new FakePaymentService(_paymentConfig.DefaultApiKey,
                _paymentConfig.DefaultEncryptionKey);

            var transaction = ToTransactionFake(Transaction, nerdsPagSvc);

            return ToTransaction(await transaction.CancelAuthorization());
        }

        public static Transaction ToTransaction(TransactionFake transaction)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                Status = (TransactionStatus) transaction.Status,
                TotalAmount = transaction.Amount,
                CardBrand = transaction.CardBrand,
                AuthorizationCode = transaction.AuthorizationCode,
                TransactionCost = transaction.Cost,
                TransactionDate = transaction.TransactionDate,
                Nsu = transaction.Nsu,
                Tid = transaction.Tid
            };
        }

        public static TransactionFake ToTransactionFake(Transaction transaction, FakePaymentService fakePaymentService)
        {
            return new TransactionFake(fakePaymentService)
            {
                Status = (TransactionStatusFake)transaction.Status,
                Amount = transaction.TotalAmount,
                CardBrand = transaction.CardBrand,
                AuthorizationCode = transaction.AuthorizationCode,
                Cost = transaction.TransactionCost,
                Nsu = transaction.Nsu,
                Tid = transaction.Tid
            };
        }
    }
}