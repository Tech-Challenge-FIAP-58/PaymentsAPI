using FCG.Payments.Models.Interfaces;
using FCG.Payments.Domain.Extensions;
using FCG.Core.Messages.Integration;
using FCG.Payments.Models.Enums;
using FCG.Core.Data.Interfaces;
using FCG.Payments.Facade;
using FCG.Payments.Models;
using FCG.Core.Mediator;

namespace FCG.Payments.Services;

public interface IPaymentService
{
    Task ProcessPayment(OrderPlacedEvent eventData);
}

public class PaymentService : IPaymentService
{
    private const int MAXATTEMPTS = 3;

    private readonly IPaymentFacade _paymentFacade;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediatorHandler _mediatorHandler;

    public PaymentService(IPaymentFacade pagamentoFacade,
        IPaymentRepository pagamentoRepository,
        IUnitOfWork unitOfWork,
        IMediatorHandler mediatorHandler)
    {
        _paymentFacade = pagamentoFacade;
        _paymentRepository = pagamentoRepository;
        _unitOfWork = unitOfWork;
        _mediatorHandler = mediatorHandler;
    }

    public async Task ProcessPayment(OrderPlacedEvent eventData)
    {
        Transaction transaction = null;

        var payments = await _paymentRepository.GetPaymentByOrderId(eventData.OrderId);
        var successfulPayment = payments
            .FirstOrDefault(x => x.Transactions
                .Any(t => t.Status == TransactionStatus.Authorized));

        // idempotência: Pagamento já em outro evento com sucesso
        if (successfulPayment is not null)
        {
            await _mediatorHandler.PublishEvent(new PaymentProcessedEvent(
                successfulPayment.OrderId,
                successfulPayment.Id,
                successfulPayment.Amount,
                PaymentResultStatus.Approved,
                null
            ));

            return;
        }

        var payment = eventData.ToPayment();

        await _unitOfWork.BeginTransactionAsync();

        for (var attempt = 1; attempt <= MAXATTEMPTS; attempt++)
        {
            transaction = await _paymentFacade.ProcessPayment(payment);

            if (transaction.Status == TransactionStatus.Authorized)
                break;
            else
                payment.AddTransaction(transaction);
        }

        payment.Process(transaction);

        _paymentRepository.AddPayment(payment);
        await _unitOfWork.CommitAsync();
    }
}
