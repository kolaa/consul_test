using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using consul_test_client.Infrastructure;

namespace consul_test_client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConsulHttpClient _consulHttpClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConsulHttpClient consulHttpClient)
        {
            _logger = logger;
            _consulHttpClient = consulHttpClient;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {

            return await _consulHttpClient.GetAsync<IEnumerable<WeatherForecast>>("test-api", "WeatherForecast");
        }
    }
}
