using System.Collections.Generic;

namespace ClientRestServer.Model
{
    public record LoadConfigurationResponseDto(
        ApplicationConfigurationDto AppConfig,
        DatabaseConfigurationDto DbConfig,
        IReadOnlyCollection<ErrorDto> Errors);

    public record LoadNodesConfigurationResponseDto(IReadOnlyCollection<NodeConfigurationDto> NodesConfiguration);
}
