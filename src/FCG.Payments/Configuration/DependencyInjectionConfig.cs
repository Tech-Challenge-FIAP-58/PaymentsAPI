using FCG.Payments.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this HostApplicationBuilder builder)
        {
            builder.Services.AddDbContext<PaymentContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
