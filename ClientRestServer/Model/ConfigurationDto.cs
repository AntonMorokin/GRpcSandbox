using System;
using System.Collections.Generic;

namespace ClientRestServer.Model
{
    public record ApplicationConfigurationDto(uint MaxThreadPoolSize, string RunningMode);

    public enum RunningMode
    {
        Dev,
        Stage,
        Prod
    }

    public record DatabaseConfigurationDto(string ConnectionString, uint TimeoutInMs);

    public record NodeConfigurationDto(
        string NodeName,
        ApplicationConfigurationDto AppConfiguration,
        DatabaseConfigurationDto DbConfiguration,
        IReadOnlyCollection<ErrorDto> Errors);
}
