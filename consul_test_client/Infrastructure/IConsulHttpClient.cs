using System;
using System.Threading.Tasks;

namespace consul_test_client.Infrastructure
{
    public interface IConsulHttpClient
    {
        Task<T> GetAsync<T>(string serviceName, string controller);
    }
}
