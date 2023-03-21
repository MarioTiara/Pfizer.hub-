using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections;
using System.Text.Json;

namespace Pfizer.hub.job
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", true, true)
                .Build();
            var serviceCollection = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog(
                    new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger()))
                .BuildServiceProvider();
            
            var logger = serviceCollection.GetService<ILogger<Program>>();
            var HubScheduler= new HubScheduler(configuration, serviceCollection);
            while(true) await HubScheduler.Run();
           
           
        }
    }
}
