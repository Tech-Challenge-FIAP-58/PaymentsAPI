using FCG.Core.Data.Interfaces;
using FCG.Core.Mediator;
using FCG.Core.Messages.Integration;
using FCG.Payments.Application.Handlers;
using FCG.Payments.Data;
using FCG.Payments.Data.Repositories;
using FCG.Payments.Facade;
using FCG.Payments.Models.Interfaces;
using FCG.Payments.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this HostApplicationBuilder builder)
        {
            builder.Services.AddDbContext<PaymentContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IMediatorHandler, MediatorHandler>();

            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IPaymentFacade, CreditCardPaymentFacade>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        }
    }
}
