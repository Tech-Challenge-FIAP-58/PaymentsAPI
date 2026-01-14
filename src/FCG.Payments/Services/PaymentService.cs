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
    Task ProcessPayment(Payment payment);
}

public class PaymentService : IPaymentService
{
    private const int MAXATTEMPTS = 3;

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

    public async Task ProcessPayment(Payment payment)
    {
        Transaction transaction = null;

        await _unitOfWork.BeginTransactionAsync();

        for (var attempt = 1; attempt <= MAXATTEMPTS; attempt++)
        {
            transaction = await _paymentFacade.ProcessPayment(payment);
            payment.AddTransaction(transaction);

            if (transaction.Status == TransactionStatus.Authorized)
                break;
        }

        payment.Process(transaction);

        _paymentRepository.AddPayment(payment);
        await _unitOfWork.CommitAsync();
    }
}
