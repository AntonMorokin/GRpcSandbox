using ClientRestServer.Configuration;
using ClientRestServer.Model;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using ConfigurationInteraction;
using System.Collections.Generic;
using System.Linq;

namespace ClientRestServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class ConfigurationController : ControllerBase
    {
        private readonly ConfigurationServer.ConfigurationServerClient _client;

        public ConfigurationController(IOptionsSnapshot<ConfigurationServerOptions> options)
        {
            var channel = GrpcChannel.ForAddress(options.Value.Url);
            _client = new ConfigurationServer.ConfigurationServerClient(channel);
        }

        [HttpGet(nameof(LoadConfigurationFromServer))]
        public async Task<ActionResult<LoadConfigurationResponseDto>> LoadConfigurationFromServer(
            [FromQuery] string ip, [FromQuery] string name)
        {

            var configRequest = new LoadConfigurationRequest
            {
                ClientMachineIp = ip,
                ClientMachineName = name
            };

            var configResponse = await _client.LoadConfigurationAsync(configRequest);

            switch (configResponse.BodyOrErrorCase)
            {
                case LoadConfigurationResponse.BodyOrErrorOneofCase.Body:
                    {
                        var appConfig = new ApplicationConfigurationDto(
                            configResponse.Body.App.MaxThreadPoolSize,
                            MapConfigurationServerRunningMode(configResponse.Body.App.Mode).ToString());

                        var dbConfig = new DatabaseConfigurationDto(
                            configResponse.Body.Database.ConnectionString,
                            configResponse.Body.Database.Timeout);

                        return new LoadConfigurationResponseDto(appConfig, dbConfig, null);
                    }
                case LoadConfigurationResponse.BodyOrErrorOneofCase.ErrorContainer:
                    {
                        var resultErrors = configResponse.ErrorContainer.Errors
                            .Select(ToClientError)
                            .ToList();

                        return StatusCode(400, new LoadConfigurationResponseDto(null, null, resultErrors));
                    };
                default:
                    return StatusCode(500);
            }
        }

        private static RunningMode MapConfigurationServerRunningMode(ApplicationConfiguration.Types.RunningMode sourceRunningMode)
        {
            switch (sourceRunningMode)
            {
                case ApplicationConfiguration.Types.RunningMode.Dev:
                    return RunningMode.Dev;
                case ApplicationConfiguration.Types.RunningMode.Stage:
                    return RunningMode.Stage;
                case ApplicationConfiguration.Types.RunningMode.Prod:
                    return RunningMode.Prod;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceRunningMode));
            }
        }

        private static ErrorDto ToClientError(Error serverError)
        {
            return new ErrorDto(serverError.Code, serverError.Message);
        }

        [HttpGet(nameof(LoadNodesConfigurationFromServer))]
        public async Task<ActionResult<LoadNodesConfigurationResponseDto>> LoadNodesConfigurationFromServer(
            [FromQuery] IEnumerable<string> nodeNames = null)
        {
            var request = new LoadNodesConfigurationRequest()
            {
                NodeNames = new StringList()
            };

            request.NodeNames.Values.AddRange(nodeNames ?? Enumerable.Empty<string>());
            
            var serverResponse = _client.LoadNodesConfiguration(request);
            var responseStream = serverResponse.ResponseStream;

            var data = new List<LoadNodesConfigurationResponse>();

            while (await responseStream.MoveNext(System.Threading.CancellationToken.None))
            {
                data.Add(responseStream.Current);
            }

            var nodesConfiguration = data.Select(ToNodeConfiguration).ToList();
            return new LoadNodesConfigurationResponseDto(nodesConfiguration);
        }

        private static NodeConfigurationDto ToNodeConfiguration(LoadNodesConfigurationResponse response)
        {
            switch (response.BodyOrErrorCase)
            {
                case LoadNodesConfigurationResponse.BodyOrErrorOneofCase.Body:
                    {
                        var appConfig = new ApplicationConfigurationDto(
                            response.Body.App.MaxThreadPoolSize,
                            MapConfigurationServerRunningMode(response.Body.App.Mode).ToString());

                        var dbConfig = new DatabaseConfigurationDto(
                            response.Body.Database.ConnectionString,
                            response.Body.Database.Timeout);

                        return new NodeConfigurationDto(response.Body.NodeName, appConfig, dbConfig, null);
                    }
                case LoadNodesConfigurationResponse.BodyOrErrorOneofCase.ErrorContainer:
                    {
                        var resultErrors = response.ErrorContainer.Errors
                            .Select(ToClientError)
                            .ToList();

                        return new NodeConfigurationDto(null, null, null, resultErrors);
                    };
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(response),
                        $"Unknown value of {nameof(LoadNodesConfigurationResponse.BodyOrErrorOneofCase)}.");
            }
        }
    }
}
