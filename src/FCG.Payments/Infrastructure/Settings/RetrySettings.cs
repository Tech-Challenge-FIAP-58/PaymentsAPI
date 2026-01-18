namespace FCG.Payments.Infrastructure.Settings
{
    public static class RetrySettings
    {
        public static int MaxRetryAttempts { get; set; }
        public static int DelayBetweenRetriesInSeconds { get; set; }
    }
}
