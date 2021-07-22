using ConfigurationInteraction;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigurationServer.Services
{
    internal sealed class ConfigurationServerImplementation
        : ConfigurationInteraction.ConfigurationServer.ConfigurationServerBase
    {
        private static readonly HashSet<string> __allowedClientIps = new()
        {
            "10.20.30.1",
            "10.20.30.15"
        };

        public override Task<LoadConfigurationResponse> LoadConfiguration(
            LoadConfigurationRequest request,
            ServerCallContext context)
        {
            bool clientIpExists = !string.IsNullOrEmpty(request.ClientMachineIp);
            bool clientNameExists = !string.IsNullOrEmpty(request.ClientMachineName);
            bool clientIpIsAllowed = clientIpExists && __allowedClientIps.Contains(request.ClientMachineIp);

            if (!(clientIpExists && clientNameExists && clientIpIsAllowed))
            {
                return GenerateErrorResponseAsync(clientIpExists, clientNameExists, clientIpIsAllowed);
            }

            return GenerateStubResponseAsync();
        }

        private Task<LoadConfigurationResponse> GenerateErrorResponseAsync(
            bool clientIpExists,
            bool clientNameExists,
            bool clientIpIsAllowed)
        {
            var errors = new List<Error>(3);

            if (!clientIpExists)
            {
                errors.Add(new Error
                {
                    Code = "1",
                    Message = $"{nameof(LoadConfigurationRequest.ClientMachineIp)} is not passed to the request."
                });
            }

            if (!clientNameExists)
            {
                errors.Add(new Error
                {
                    Code = "2",
                    Message = $"{nameof(LoadConfigurationRequest.ClientMachineName)} is not passed to the request."
                });
            }

            if (!clientIpIsAllowed)
            {
                errors.Add(new Error
                {
                    Code = "3",
                    Message = $"Passed {nameof(LoadConfigurationRequest.ClientMachineIp)} is not allowed."
                });
            }

            var response = new LoadConfigurationResponse
            {
                ErrorContainer = new ErrorContainer()
            };
            response.ErrorContainer.Errors.AddRange(errors);

            return ValueTask.FromResult(response).AsTask();
        }

        private Task<LoadConfigurationResponse> GenerateStubResponseAsync()
        {
            var response = new LoadConfigurationResponse
            {
                Body = new LoadConfigurationResponseBody
                {
                    App = new ApplicationConfiguration
                    {
                        MaxThreadPoolSize = 2048,
                        Mode = ApplicationConfiguration.Types.RunningMode.Stage
                    },
                    Database = new DatabaseConfiguration
                    {
                        ConnectionString = "tcp://localhost:5678;abc@def",
                        Timeout = (uint)TimeSpan.FromSeconds(60).TotalMilliseconds
                    }
                }
            };

            return ValueTask.FromResult(response).AsTask();
        }
    }
}
