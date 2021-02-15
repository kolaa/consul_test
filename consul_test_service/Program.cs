using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Linq;
using System.Net;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;


namespace consul_test_service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostingEnvironment hostingEnvironment = null;  
            
            return 
                Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder => { 
                        webBuilder
                            .UseContentRoot(Directory.GetCurrentDirectory())  
                            .ConfigureServices(  
                                services =>  
                                {  
                                    hostingEnvironment = services  
                                        .Where(x => x.ServiceType == typeof(IHostingEnvironment))  
                                        .Select(x => (IHostingEnvironment)x.ImplementationInstance)  
                                        .First();                        
  
                                })  
                            .UseKestrel(options =>  
                            {  
                                var config = new ConfigurationBuilder()  
                                    .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: false)  
                                    .Build();  
                                options.Listen(IPAddress.Loopback, config.GetValue<int>("Host:Port"));  
                            })
                            .UseStartup<Startup>(); });
        }
    }
}
