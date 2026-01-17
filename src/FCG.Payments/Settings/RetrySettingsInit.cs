using FCG.Core.Objects;

namespace FCG.Payments.Configuration;

public static class RetrySettingsInit
{
    public static void InitilizeRetrySettings(this HostApplicationBuilder builder)
    {
        RetrySettings.MaxRetryAttempts = builder.Configuration.GetValue<int>("RetrySettings:MaxRetryAttempts");
        RetrySettings.DelayBetweenRetriesInSeconds = builder.Configuration.GetValue<int>("RetrySettings:DelayBetweenRetriesInSeconds");
    }
}
