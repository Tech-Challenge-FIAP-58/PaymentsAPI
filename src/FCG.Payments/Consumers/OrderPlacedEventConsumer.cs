using FCG.Core.Messages.Integration;
using MassTransit;

namespace FCG.Payments.Consumers
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly ILogger<OrderPlacedEventConsumer> _logger;

        public OrderPlacedEventConsumer(ILogger<OrderPlacedEventConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            Console.WriteLine($"[Payments] Message received: {context.Message.OrderId}");

            await context.Publish(new PaymentProcessedEvent(
                context.Message.ClientId,
                context.Message.OrderId
            ));
        }
    }
}
