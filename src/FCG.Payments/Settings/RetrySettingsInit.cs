using FCG.Core.Objects;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.Settings;

public static class RetrySettingsInit
{
    [ExcludeFromCodeCoverage]
    public static void InitilizeRetrySettings(this HostApplicationBuilder builder)
    {
        RetrySettings.MaxRetryAttempts = builder.Configuration.GetValue<int>("RetrySettings:MaxRetryAttempts");
        RetrySettings.DelayBetweenRetriesInSeconds = builder.Configuration.GetValue<int>("RetrySettings:DelayBetweenRetriesInSeconds");
    }
}
