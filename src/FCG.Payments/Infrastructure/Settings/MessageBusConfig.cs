using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Infrastructure.Settings
{
    [ExcludeFromCodeCoverage]
    public static class MessageBusConfig
    {
        public static void AddMessageBusConfiguration(this HostApplicationBuilder builder)
        {
            builder.Services.Configure<RabbitMqSettings>(
                builder.Configuration.GetSection("RabbitMQ"));
        }
    }
}
