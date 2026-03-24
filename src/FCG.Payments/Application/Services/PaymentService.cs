using FCG.Payments.Facade;
using FCG.Payments.Application.Mediator;
using FCG.Payments.Domain.Events;
using FCG.Payments.Application.Extensions;
using FCG.Payments.Domain.Entities.Interfaces;
using FCG.Payments.Domain.Entities.Enums;
using FCG.Payments.Domain.Entities;
using FCG.Payments.Application.Interfaces;
using FCG.Core.Integration;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;


namespace FCG.Payments.Application.Services;



public interface IPaymentService
{
    Task ProcessPayment(OrderPlacedEvent eventData);
}

public class PaymentService : IPaymentService
{
    private static readonly HttpClient DefaultHttpClient = new HttpClient
    {
        BaseAddress = new Uri("https://zq0beaods2.execute-api.us-east-2.amazonaws.com/default/fcgPaymentLambda")
    };


    private const int MAXATTEMPTS = 3;

    private readonly IPaymentFacade _paymentFacade;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediatorHandler _mediatorHandler;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentFacade pagamentoFacade,
        IPaymentRepository pagamentoRepository,
        IUnitOfWork unitOfWork,
        IMediatorHandler mediatorHandler,
        ILogger<PaymentService> logger,
        HttpClient? httpClient = null)
    {
        _paymentFacade = pagamentoFacade;
        _paymentRepository = pagamentoRepository;
        _unitOfWork = unitOfWork;
        _mediatorHandler = mediatorHandler;
        _httpClient = httpClient ?? DefaultHttpClient;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessPayment(OrderPlacedEvent eventData)
    {
        Transaction transaction = null;

        var payments = await _paymentRepository.GetPaymentByOrderId(eventData.OrderId);
        var successfulPayment = payments
            .FirstOrDefault(x => x.Transactions
                .Any(t => t.Status == TransactionStatus.Authorized));

        if (successfulPayment is not null)
        {
            await _mediatorHandler.PublishEvent(new PaymentProcessedDomainEvent(
                successfulPayment.OrderId,
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
            var json = JsonConvert.SerializeObject(eventData);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
            
                _logger.LogError("Erro na Lambda: {Status} - {Body}", response.StatusCode, error);
            
                return; // evita retry infinito
            }

            var body = await response.Content.ReadAsStringAsync();

            transaction = JsonConvert.DeserializeObject<Transaction>(body);

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
