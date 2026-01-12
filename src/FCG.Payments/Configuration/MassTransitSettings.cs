using FCG.Payments.Configuration.Dtos;
using Microsoft.Extensions.Options;
using FCG.Payments.Consumers;
using MassTransit;

namespace FCG.Payments.Configuration
{
    public static class MassTransitSettings
    {
        public static HostApplicationBuilder AddMassTransitSettings(this HostApplicationBuilder builder)
        {
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderPlacedEventConsumer>();

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

                    cfg.ConfigureEndpoints(context);
                });
            });

            return builder;
        }
    }
}
