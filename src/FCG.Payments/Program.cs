using FCG.Payments.Configuration;
using FCG.Payments.Facade;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<PaymentConfig>(builder.Configuration.GetSection("PaymentConfig"));

builder.RegisterServices();

builder.AddMessageBusConfiguration();

builder.AddMassTransitSettings();

builder.RegisterServices();

builder.InitilizeRetrySettings();

var host = builder.Build();

host.Run();
