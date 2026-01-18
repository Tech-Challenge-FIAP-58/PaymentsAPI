using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using FGC.Publisher.Test;
using MassTransit;
using FCG.Core.Contracts;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqSettings>(
            context.Configuration.GetSection("RabbitMQ")
        );

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                var settings = ctx
                    .GetRequiredService<IOptions<RabbitMqSettings>>()
                    .Value;

                cfg.Host(new Uri(
                    $"rabbitmq://{settings.Host}:{settings.Port}{settings.VirtualHost}"
                ), h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });
            });
        });
    })
    .Build();

await host.StartAsync();

// ============================
// PUBLICAÇÃO DO EVENTO
// ============================
var bus = host.Services.GetRequiredService<IBus>();

var message = new OrderPlacedEvent(
    clientId: 1,
    orderId: new Guid(),
    paymentMethod: 1,
    amount: 250.75m,
    cardName: "JOAO A SILVA",
    cardNumber: "4532123456789012",
    expirationDate: "12/28",
    cvv: "123"
);

Console.WriteLine("[Publisher] Publishing OrderPlacedEvent");

await bus.Publish(message);

Console.WriteLine("[Publisher] Event published");

await host.StopAsync();
