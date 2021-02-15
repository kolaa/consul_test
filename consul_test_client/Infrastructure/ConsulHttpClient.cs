using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Consul;

namespace consul_test_client.Infrastructure
{
    public class ConsulHttpClient: IConsulHttpClient
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConsulClient _consulClient;

        public ConsulHttpClient(IHttpClientFactory clientFactory, IConsulClient consulClient)
        {
            _clientFactory = clientFactory;
            _consulClient = consulClient;
        }

        public async Task<T> GetAsync<T>(string serviceName, string controller)
        {
            var uri = await GetRequestUriAsync(serviceName, controller);

            var client = _clientFactory.CreateClient();

            var response = await client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                return default(T);
            }

            var content = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<T>(content);
        }

        private async Task<Uri> GetRequestUriAsync(string serviceName, string controller)
        {
            //Get all services registered on Consul
            var allRegisteredServices = await _consulClient.Agent.Services();

            //Get all instance of the service went to send a request to
            var registeredServices = allRegisteredServices.Response?.Where(s => s.Value.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList();

            //Get a random instance of the service
            var service = GetRandomInstance(registeredServices, serviceName);

            if (service == null)
            {
                throw new Exception($"Consul service: '{serviceName}' was not found.");
            }

            var uriBuilder = new UriBuilder(service.Address +"/"+ controller);

            return uriBuilder.Uri;
        }

        private AgentService GetRandomInstance(IList<AgentService> services, string serviceName)
        {
            var random = new Random();

            var serviceToUse = services[random.Next(0, services.Count)];

            return serviceToUse;
        }
    }

}