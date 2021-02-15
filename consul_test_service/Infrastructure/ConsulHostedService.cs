using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace consul_test_service.Infrastructure
{
    public class ConsulHostedService : IHostedService
    {
        private CancellationTokenSource _cts;
        private readonly IConsulClient _consulClient;
        private readonly IOptions<ConsulConfig> _consulConfig;
        private readonly IOptions<HostConfig> _hostConfig;
        private readonly ILogger<ConsulHostedService> _logger;
        private readonly IServer _server;
        private string _registrationID;

        public ConsulHostedService(IConsulClient consulClient, IOptions<ConsulConfig> consulConfig, IOptions<HostConfig> hostConfig, ILogger<ConsulHostedService> logger, IServer server)
        {
            _server = server;
            _logger = logger;
            _consulConfig = consulConfig;
            _hostConfig = hostConfig;
            _consulClient = consulClient;

        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var hostName = System.Net.Dns.GetHostName();

            var tcpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                Interval = TimeSpan.FromSeconds(30),
                TCP = $"{IPAddress.Loopback}:{_hostConfig.Value.Port}"
            };

            var httpCheck = new AgentServiceCheck()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                Interval = TimeSpan.FromSeconds(30),
                HTTP = $"http://{IPAddress.Loopback}:{_hostConfig.Value.Port}/HealthCheck"
            };

            _registrationID = $"{_consulConfig.Value.ServiceID}-{hostName}";

            var registration = new AgentServiceRegistration()
            {
                Checks = new[] { tcpCheck, httpCheck },
                ID = _registrationID,
                Name = _consulConfig.Value.ServiceName,
                Address = $"http://{hostName}/{_hostConfig.Value.Location}",
                Tags = new[] {"Students", "Courses", "School"}
            };

            _logger.LogInformation("Registering in Consul");
            await _consulClient.Agent.ServiceDeregister(registration.ID, _cts.Token);
            await _consulClient.Agent.ServiceRegister(registration, _cts.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _logger.LogInformation("Deregistering from Consul");
            try
            {
                await _consulClient.Agent.ServiceDeregister(_registrationID, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Deregisteration failed");
            }
        }
    }
}
