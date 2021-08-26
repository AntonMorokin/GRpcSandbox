using System;
using System.Collections.Generic;

namespace ClientRestServer.Model
{
    public record ConfigurationResponseDto(
        ApplicationConfigurationDto AppConfig,
        DatabaseConfigurationDto DbConfig,
        IReadOnlyCollection<ErrorDto> Errors);

    public record ApplicationConfigurationDto(uint MaxThreadPoolSize, string RunningMode);

    public enum RunningMode
    {
        Dev,
        Stage,
        Prod
    }

    public record DatabaseConfigurationDto(string ConnectionString, TimeSpan Timeout);

    public record ErrorDto(string Code, string Message);
}
