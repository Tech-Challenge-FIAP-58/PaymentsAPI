using FCG.Payments.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.RegisterServices();

builder.AddMessageBusConfiguration();

builder.AddMassTransitSettings();

builder.RegisterServices();

builder.InitilizeRetrySettings();

var host = builder.Build();

host.Run();
