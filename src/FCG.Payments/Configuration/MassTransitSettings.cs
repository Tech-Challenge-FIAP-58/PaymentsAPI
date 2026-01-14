using FCG.Core.Messages.Integration;
using FCG.Payments.Configuration.Dtos;
using FCG.Payments.Consumers;
using MassTransit;
using Microsoft.Extensions.Options;

namespace FCG.Payments.Configuration
{
    public static class MassTransitSettings
    {
        public static HostApplicationBuilder AddMassTransitSettings(this HostApplicationBuilder builder)
        {
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderPaymentRequestedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitSettings = context
                        .GetRequiredService<IOptions<RabbitMqSettings>>()
                        .Value;

                    cfg.Host(rabbitSettings.Host, rabbitSettings.VirtualHost, h =>
                    {
                        h.Username(rabbitSettings.Username);
                        h.Password(rabbitSettings.Password);
                    });

                    cfg.ReceiveEndpoint("payment-processed-debug-queue", e =>
                    {
                        e.Bind<PaymentProcessedIntegrationEvent>();
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return builder;
        }
    }
}
