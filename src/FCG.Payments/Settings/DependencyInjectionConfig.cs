using FCG.Core.Data.Interfaces;
using FCG.Core.Mediator;
using FCG.Payments.Data;
using FCG.Payments.Data.Repositories;
using FCG.Payments.Facade;
using FCG.Payments.Models.Interfaces;
using FCG.Payments.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Settings
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
