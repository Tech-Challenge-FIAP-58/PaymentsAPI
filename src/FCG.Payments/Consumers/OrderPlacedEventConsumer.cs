using FCG.Core.Messages.Integration;
using FCG.Payments.Models.Enums;
using FCG.Payments.Services;
using FCG.Payments.Models;
using MassTransit;

namespace FCG.Payments.Consumers;

public class OrderPlacedEventConsumer
    : IConsumer<OrderPlacedEvent>
{
    private readonly IPaymentService _paymentService;

    public OrderPlacedEventConsumer(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task Consume(
        ConsumeContext<OrderPlacedEvent> context)
    {
        await _paymentService.ProcessPayment(
            MapToPayment(context.Message)
        );
    }

    private static Payment MapToPayment(
        OrderPlacedEvent message)
    {
        return new Payment(
                        message.OrderId,
                        (PaymentMethod)message.PaymentMethod,
                        message.Amount,
                        new CreditCard(
                            message.CardName,
                            message.CardNumber,
                            message.ExpirationDate,
                            message.Cvv)
                    );
    }
}
