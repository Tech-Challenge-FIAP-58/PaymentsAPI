using FCG.Payments.Settings.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Settings
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
