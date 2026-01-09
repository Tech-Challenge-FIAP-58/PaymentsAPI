using FCG.Payments;
using FCG.Payments.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.RegisterServices();

builder.AddMessageBusConfiguration();

builder.InitilizeRetrySettings();

var host = builder.Build();

host.Run();
