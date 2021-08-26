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
        private readonly IOptionsSnapshot<ConfigurationServerOptions> _options;

        public ConfigurationController(IOptionsSnapshot<ConfigurationServerOptions> options)
        {
            _options = options;
        }

        [HttpGet(nameof(LoadConfigurationFromServer))]
        public async Task<ActionResult<ConfigurationResponseDto>> LoadConfigurationFromServer(
            [FromQuery] string ip, [FromQuery] string name)
        {
            var channel = GrpcChannel.ForAddress(_options.Value.Url);
            var client = new ConfigurationServer.ConfigurationServerClient(channel);

            var configRequest = new LoadConfigurationRequest
            {
                ClientMachineIp = ip,
                ClientMachineName = name
            };

            var configResponse = await client.LoadConfigurationAsync(configRequest);

            switch (configResponse.BodyOrErrorCase)
            {
                case LoadConfigurationResponse.BodyOrErrorOneofCase.Body:
                    {
                        var appConfig = new ApplicationConfigurationDto(
                            configResponse.Body.App.MaxThreadPoolSize,
                            MapConfigurationServerRunningMode(configResponse.Body.App.Mode).ToString());

                        var dbConfig = new DatabaseConfigurationDto(
                            configResponse.Body.Database.ConnectionString,
                            TimeSpan.FromMilliseconds(configResponse.Body.Database.Timeout));

                        return new ConfigurationResponseDto(appConfig, dbConfig, null);
                    }
                case LoadConfigurationResponse.BodyOrErrorOneofCase.ErrorContainer:
                    {
                        var resultErrors = configResponse.ErrorContainer.Errors
                            .Select(e => new ErrorDto(e.Code, e.Message)).ToList();

                        return StatusCode(400, new ConfigurationResponseDto(null, null, resultErrors));
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
    }
}
