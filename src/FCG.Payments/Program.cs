using FCG.Payments.Data;
using FCG.Payments.Facade;
using FCG.Payments.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[assembly: ExcludeFromCodeCoverage]

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<PaymentConfig>(builder.Configuration.GetSection("PaymentConfig"));

builder.RegisterServices();

builder.AddMessageBusConfiguration();

builder.AddMassTransitSettings();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

builder.InitilizeRetrySettings();

var host = builder.Build();

#region MIGRATION COM RETRY
// Observação: este bloco roda **antes** do servidor iniciar. Ele tenta aplicar
// migrations até 'maxAttempts' vezes, com backoff exponencial (limitado).
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<PaymentContext>();

    const int maxAttempts = 10;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            logger.LogInformation("Tentando aplicar migrations (tentativa {Attempt}/{MaxAttempts})...", attempt, maxAttempts);
            dbContext.Database.Migrate(); // aplica migrations pendentes (síncrono)
            logger.LogInformation("Migrations aplicadas com sucesso.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao aplicar migrations na tentativa {Attempt}.", attempt);
            if (attempt == maxAttempts)
            {
                logger.LogError(ex, "Não foi possível aplicar migrations após {MaxAttempts} tentativas. Encerrando aplicação.", maxAttempts);
                throw; // aborta a inicialização (você pode optar por não lançar e continuar)
            }
            // backoff simples (2s * attempt), limitado a 30s
            var delay = TimeSpan.FromSeconds(Math.Min(30, 2 * attempt));
            logger.LogInformation("Aguardando {Delay} antes da próxima tentativa...", delay);
            // usa Task.Delay para não bloquear a thread
            await Task.Delay(delay);
        }
    }
}
#endregion

host.Run();
