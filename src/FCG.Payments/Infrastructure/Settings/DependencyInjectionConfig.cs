using FCG.Payments.Infrastructure.Persistence.Repositories;
using FCG.Payments.Domain.Entities.Interfaces;
using FCG.Payments.Infrastructure.Persistence;
using FCG.Payments.Application.Interfaces;
using FCG.Payments.Application.Mediator;
using FCG.Payments.Application.Services;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using FCG.Payments.Facade;

namespace FCG.Payments.Infrastructure.Settings
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this HostApplicationBuilder builder)
        {
            builder.Services.AddDbContext<PaymentContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Core")));

            builder.Services.AddScoped<IMediatorHandler, MediatorHandler>();

            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IPaymentFacade, CreditCardPaymentFacade>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        }
    }
}
