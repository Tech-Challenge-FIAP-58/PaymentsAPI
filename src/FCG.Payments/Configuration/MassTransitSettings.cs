using FCG.Core.Objects;
using MassTransit;

namespace FCG.Payments.Configuration
{
    public static class MassTransitSettings
    {
        public static HostApplicationBuilder AddMassTransitSettings(this HostApplicationBuilder builder)
        {
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<Worker>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var awsSettings = context.GetRequiredService<IOptions<AppSettingsAWS>>().Value;

                    if (builder.Environment.IsDevelopment())
                    {
                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
                        });
                    }
                    else
                    {
                        // Configuração de produção
                        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost",
                                 builder.Configuration["RabbitMQ:VirtualHost"] ?? "/", h =>
                                 {
                                     h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                                     h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");

                                     // Se usar porta customizada
                                     if (int.TryParse(builder.Configuration["RabbitMQ:Port"], out var port))
                                     {
                                         h.Port = port;
                                     }

                                     // Se usar SSL/TLS
                                     if (bool.TryParse(builder.Configuration["RabbitMQ:UseSsl"], out var useSsl) && useSsl)
                                     {
                                         h.UseSsl(s =>
                                         {
                                             s.ServerName = builder.Configuration["RabbitMQ:Host"];
                                         });
                                     }
                                 });
                    }

                    cfg.ReceiveEndpoint(awsSettings.ConsumerQueueName, e =>
                    {
                        e.UseMessageRetry(r =>
                        {
                            r.Incremental(RetrySettings.MaxRetryAttempts,
                                          TimeSpan.FromSeconds(RetrySettings.DelayBetweenRetriesInSeconds),
                                          TimeSpan.FromSeconds(2));
                        });

                        if (builder.Environment.IsDevelopment())
                        {
                            e.PrefetchCount = 1;
                            e.ConcurrentMessageLimit = 1;
                        }
                        else
                        {
                            e.PrefetchCount = 5;
                            e.ConcurrentMessageLimit = 5;
                        }

                        e.ClearSerialization();
                        e.UseNewtonsoftRawJsonSerializer();
                        e.ConfigureConsumer<Worker>(context);
                    });
                });

                var awsSettingsSection = builder.Configuration.GetSection("AppSettingsAWS");
                var nomeDaFila = awsSettingsSection["ProducerQueueName"];
                EndpointConvention.Map<ImportacaoMessage>(new Uri($"queue:{nomeDaFila}"));
            });

            return builder;
        }
    }
}
