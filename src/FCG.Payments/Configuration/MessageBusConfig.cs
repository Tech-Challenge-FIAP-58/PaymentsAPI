using FCG.Payments.Configuration.Dtos;

namespace FCG.Payments.Configuration
{
    public static class MessageBusConfig
    {
        public static void AddMessageBusConfiguration(this HostApplicationBuilder builder)
        {
            builder.Services.Configure<RabbitMqSettings>(
                builder.Configuration.GetSection("RabbitMQ"));
        }
    }
}
