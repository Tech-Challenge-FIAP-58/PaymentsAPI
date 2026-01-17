using FCG.Core.Messages.Integration;
using FCG.Payments.Services;
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
        await _paymentService.ProcessPayment(context.Message);
    }
}
