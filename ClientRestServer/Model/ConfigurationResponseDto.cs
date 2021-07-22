using System;

namespace ClientRestServer.Model
{
    public record ConfigurationResponseDto(ApplicationConfigurationDto AppConfig, DatabaseConfigurationDto DbConfig);

    public record ApplicationConfigurationDto(uint MaxThreadPoolSize, string RunningMode);

    public enum RunningMode
    {
        Dev,
        Stage,
        Prod
    }

    public record DatabaseConfigurationDto(string ConnectionString, TimeSpan Timeout);
}
