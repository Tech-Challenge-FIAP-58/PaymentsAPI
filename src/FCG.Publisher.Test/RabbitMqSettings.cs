namespace FGC.Publisher.Test;

public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
