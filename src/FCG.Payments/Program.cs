using FCG.Payments;
using FCG.Payments.Configuration;
using FCG.Payments.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.RegisterServices();
builder.AddMessageBusConfiguration();

var host = builder.Build();
host.Run();
