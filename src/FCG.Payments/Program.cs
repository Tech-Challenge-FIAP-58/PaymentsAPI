using FCG.Payments.Configuration;
using FCG.Payments.Facade;
using MassTransit;
using MediatR;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<PaymentConfig>(builder.Configuration.GetSection("PaymentConfig"));

builder.RegisterServices();

builder.AddMessageBusConfiguration();

builder.AddMassTransitSettings();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

builder.RegisterServices();

builder.InitilizeRetrySettings();

var host = builder.Build();

host.Run();
