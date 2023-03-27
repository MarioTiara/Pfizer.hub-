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
using System.Runtime.InteropServices;

namespace Pfizer.hub.job
{
    class Program
    {

    static async Task Main(string[] args)
    {
        var configuration = BuildConfig();
        var BackDate=configuration.GetValue<string>("BackDate");
        if (!String.IsNullOrWhiteSpace(BackDate)){
            string[] DateUpdate=BackDate.Split("|");
            foreach (var d in DateUpdate){
                await run (d);
            }

        }else{
          await run (DateTime.Now.ToString());
        }
       
    }
        
        public static async Task run (string DateUpdate){
            Console.WriteLine("run");
            var configuration = BuildConfig();
            var serviceCollection= BuildServiceCollection(configuration);
            var auth= new Authorization(configuration, serviceCollection);
            await auth.Authorize();

            string Token= await auth.GetBeareToken();
            var PizerStock= new PizerStock(configuration, serviceCollection, Token);
            var Stocks= await PizerStock.GetSavingStock(DateUpdate);
            var SavingHub= new SavingHub(configuration, serviceCollection, Token);
            await SavingHub.SendStocks(Stocks, DateUpdate);
        }
        public static  IConfigurationRoot BuildConfig(){
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", true, true)
                .Build();

                return configuration;
        }

        public static ServiceProvider BuildServiceCollection(IConfigurationRoot configuration){
              var serviceCollection = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog(
                    new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger()))
                .BuildServiceProvider();
            return  serviceCollection;
        }
    }
}
