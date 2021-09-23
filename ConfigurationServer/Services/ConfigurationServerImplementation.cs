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

        private static readonly Random rn = new();

        public override Task<LoadConfigurationResponse> LoadConfiguration(
            LoadConfigurationRequest request,
            ServerCallContext context)
        {
            bool clientIpExists = !string.IsNullOrEmpty(request.ClientMachineIp);
            bool clientNameExists = !string.IsNullOrEmpty(request.ClientMachineName);
            bool clientIpIsAllowed = clientIpExists && __allowedClientIps.Contains(request.ClientMachineIp);

            if (!(clientIpExists && clientNameExists && clientIpIsAllowed))
            {
                return GenerateLoadConfigurationErrorResponseAsync(clientIpExists, clientNameExists, clientIpIsAllowed);
            }

            return GenerateLoadConfigurationStubResponseAsync();
        }

        private Task<LoadConfigurationResponse> GenerateLoadConfigurationErrorResponseAsync(
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

        private Task<LoadConfigurationResponse> GenerateLoadConfigurationStubResponseAsync()
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

        public override async Task LoadNodesConfiguration(
            LoadNodesConfigurationRequest request,
            IServerStreamWriter<LoadNodesConfigurationResponse> responseStream,
            ServerCallContext context)
        {
            var data = request.NodeNames?.Values == null || request.NodeNames.Values.Count == 0
                ? GenerateLoadNodesConfigurationStubResponse()
                : GenerateLoadNodesConfigurationStubResponse(request.NodeNames.Values);

            foreach (var item in data)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(rn.Next(1000));
                await responseStream.WriteAsync(new LoadNodesConfigurationResponse
                {
                    Body = item
                });
            }
        }

        private static IEnumerable<LoadNodesConfigurationResponseBody> GenerateLoadNodesConfigurationStubResponse()
        {
            int n = rn.Next(10) + 1;
            for (int i = 0; i < n; i++)
            {
                yield return GenerateLoadNodesConfigurationResponseBodyWithRandomValues(i + 1);
            }
        }

        private static LoadNodesConfigurationResponseBody GenerateLoadNodesConfigurationResponseBodyWithRandomValues(
            int nodeNumber,
            string nodeName = null)
        {
            return new LoadNodesConfigurationResponseBody
            {
                NodeName = nodeName ?? $"Node #{nodeNumber}",
                App = new ApplicationConfiguration
                {
                    MaxThreadPoolSize = (uint)(rn.Next(1024) + 32),
                    Mode = (ApplicationConfiguration.Types.RunningMode)rn.Next(3)
                },
                Database = new DatabaseConfiguration
                {
                    ConnectionString = $"tcp://{nodeName ?? "localhost"}:{rn.Next(9999) + 1000};abc@def",
                    Timeout = (uint)TimeSpan.FromSeconds(rn.Next(30) + 1).TotalMilliseconds
                }
            };
        }

        private static IEnumerable<LoadNodesConfigurationResponseBody> GenerateLoadNodesConfigurationStubResponse(
            IEnumerable<string> nodeNames)
        {
            int i = 1;
            foreach (var nodeName in nodeNames)
            {
                yield return GenerateLoadNodesConfigurationResponseBodyWithRandomValues(i++, nodeName);
            }
        }
    }
}
