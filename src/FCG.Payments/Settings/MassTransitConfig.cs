using FCG.Core.Messages.Integration;
using FCG.Core.Objects;
using FCG.Payments.Consumers;
using FCG.Payments.Settings.Dtos;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Settings
{
    public static class MassTransitConfig
    {
        [ExcludeFromCodeCoverage]
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

                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(
                            RetrySettings.MaxRetryAttempts,
                            TimeSpan.FromSeconds(RetrySettings.DelayBetweenRetriesInSeconds)
                        );
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return builder;
        }
    }
}
