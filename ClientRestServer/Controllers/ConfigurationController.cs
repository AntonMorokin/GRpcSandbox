using ClientRestServer.Configuration;
using ClientRestServer.Model;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using ConfigurationInteraction;

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
        public async Task<ActionResult<ConfigurationResponseDto>> LoadConfigurationFromServer()
        {
            var channel = GrpcChannel.ForAddress(_options.Value.Url);
            var client = new ConfigurationServer.ConfigurationServerClient(channel);

            var configRequest = new LoadConfigurationRequest
            {
                ClientMachineIp = "",
                ClientMachineName = ""
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

                        return new ConfigurationResponseDto(appConfig, dbConfig);
                    }
                case LoadConfigurationResponse.BodyOrErrorOneofCase.ErrorContainer:
                    return StatusCode(400);
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
