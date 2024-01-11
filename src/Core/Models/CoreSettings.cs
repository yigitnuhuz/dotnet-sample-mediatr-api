using System.Collections.Generic;

namespace Core.Models
{
    public record CoreSettings
    {
        public string Name { get; init; }
        public string Version { get; init; }
        public SeqSettings Seq { get; init; }
        public CacheSettings Cache { get; init; }
        public LocalizationSettings Localization { get; init; }
        public AuthSettings Auth { get; init; }
        public SentrySettings Sentry { get; init; }
        public DatadogSettings Datadog { get; init; }
    }

    public record SentrySettings(bool IsActive, string Dsn, bool Debug);

    public record DatadogSettings(bool IsActive, string ServiceName);

    public record SeqSettings(bool IsActive, string SeqApiKey, string SeqServerUrl);

    public record CacheSettings(bool IsActive, string RedisInstanceName, string RedisConfiguration);

    public record LocalizationSettings(string HeaderName, string DefaultCulture, List<string> SupportedCultures);

    public record AuthSettings(string System, string JwtSecret, int TokenDuration);
}