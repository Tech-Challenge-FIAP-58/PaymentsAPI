using FCG.Payments.Data;
using FCG.Payments.Data.Repositories;
using FCG.Payments.Facade;
using FCG.Payments.Models.Interfaces;
using FCG.Payments.Services;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this HostApplicationBuilder builder)
        {
            builder.Services.AddDbContext<PaymentContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IPaymentFacade, CreditCardPaymentFacade>();

            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        }
    }
}
