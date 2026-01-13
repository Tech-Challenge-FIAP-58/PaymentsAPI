using FCG.Core.Messages;
using FCG.Core.Messages.Integration;
using FCG.Payments.Models;
using FCG.Payments.Models.Enums;
using FCG.Payments.Services;
using MassTransit;

namespace FCG.Payments.Consumers
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly ILogger<OrderPlacedEventConsumer> _logger;
        private readonly IPaymentService _paymentService;

        public OrderPlacedEventConsumer(ILogger<OrderPlacedEventConsumer> logger, 
            IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            Console.WriteLine($"[Payments] Message received: {context.Message.OrderId}");

            var message = context.Message;

            var payment = new Payment
            {
                OrderId = message.OrderId,
                PaymentType = (PaymentMethod)message.PaymentType,
                Amount = message.Amount,
                CreditCard = new CreditCard(
                    message.CardName, message.CardNumber, message.ExpirationDate, message.Cvv)
            };

            var result = await _paymentService.AuthorizePayment(payment);

            await context.Publish(new PaymentProcessedEvent(
                context.Message.ClientId,
                context.Message.OrderId
            ));
        }
    }
}
