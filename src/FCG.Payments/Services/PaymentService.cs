using FCG.Payments.Models.Interfaces;
using FCG.Core.Messages.Integration;
using FCG.Payments.Models.Enums;
using FluentValidation.Results;
using FCG.Core.Data.Interfaces;
using FCG.Core.DomainObjects;
using FCG.Payments.Facade;
using FCG.Payments.Models;

namespace FCG.Payments.Services;

public interface IPaymentService
{
    Task<ResponseMessage> AuthorizePayment(Payment payment);
    Task<ResponseMessage> HandlerPayment(Guid orderId);
    Task<ResponseMessage> CancelPayment(Guid orderId);
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentFacade _paymentFacade;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IPaymentFacade pagamentoFacade,
        IPaymentRepository pagamentoRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentFacade = pagamentoFacade;
        _paymentRepository = pagamentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseMessage> AuthorizePayment(Payment payment)
    {
        var transaction = await _paymentFacade.AuthorizePayment(payment);
        var validationResult = new ValidationResult();

        if (transaction.Status != TransactionStatus.Authorized)
        {
            validationResult.Errors.Add(new ValidationFailure("Payment",
                    "Payment refused, Please contact your card issuer"));

            return new ResponseMessage(validationResult);
        }

        payment.AddTransaction(transaction);
        _paymentRepository.AddPayment(payment);

        if (!await _unitOfWork.CommitAsync())
        {
            validationResult.Errors.Add(new ValidationFailure("Payment",
                "An error occurred while processing the payment"));

            // Cancel payment in gateway
            await CancelPayment(payment.OrderId);

            return new ResponseMessage(validationResult);
        }

        return new ResponseMessage(validationResult);
    }

    public async Task<ResponseMessage> HandlerPayment(Guid orderId)
    {
        var transactions = await _paymentRepository.GetTransactionsByOrderId(orderId);
        var authorizedTransaction = transactions?.FirstOrDefault(t => t.Status == TransactionStatus.Authorized);
        var validationResult = new ValidationResult();

        if (authorizedTransaction == null) throw new DomainException($"Transaction not found for the order {orderId}");

        var transaction = await _paymentFacade.HandlerPayment(authorizedTransaction);

        if (transaction.Status != TransactionStatus.Paid)
        {
            validationResult.Errors.Add(new ValidationFailure("Payment",
                $"An error occurred while processing the payment for the order {orderId}"));

            return new ResponseMessage(validationResult);
        }

        transaction.PaymentId = authorizedTransaction.PaymentId;
        _paymentRepository.AddTransaction(transaction);

        if (!await _unitOfWork.CommitAsync())
        {
            validationResult.Errors.Add(new ValidationFailure("Payment",
                $"Could not persist the payment capture for order {orderId}"));

            return new ResponseMessage(validationResult);
        }

        return new ResponseMessage(validationResult);
    }

    public async Task<ResponseMessage> CancelPayment(Guid orderId)
    {
        var transactions = await _paymentRepository.GetTransactionsByOrderId(orderId);
        var authorizedTransaction = transactions?.FirstOrDefault(t => t.Status == TransactionStatus.Authorized);
        var validationResult = new ValidationResult();

        if (authorizedTransaction == null) throw new DomainException($"Transaction not found for the order {orderId}");

        var transaction = await _paymentFacade.CancelAuthorization(authorizedTransaction);

        if (transaction.Status != TransactionStatus.Cancelled)
        {
            validationResult.Errors.Add(new ValidationFailure("Payment",
                $"An error occurred while canceling the payment for the order {orderId}"));

            return new ResponseMessage(validationResult);
        }

        transaction.PaymentId = authorizedTransaction.PaymentId;
        _paymentRepository.AddTransaction(transaction);

        if (!await _unitOfWork.CommitAsync())
        {
            validationResult.Errors.Add(new ValidationFailure("Payment",
                $"Could not persist the payment cancellation for order {orderId}"));

            return new ResponseMessage(validationResult);
        }

        return new ResponseMessage(validationResult);
    }
}
