using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pfizer.hub.job
{
    public class HubScheduler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private List<string> _timeSchedule;
        public HubScheduler(IConfiguration configuration, IServiceProvider services){
            _configuration=configuration;
            _timeSchedule=_configuration.GetSection("TimeSchedule").Value.Split("|").ToList();
            _logger=services.GetService<ILogger<HubScheduler>>();
        }

        public async Task Run(){
            if(_timeSchedule.Contains(DateTime.Now.ToString("hh:mm tt"))){
                 _logger.LogInformation($"Not in Schedule: {DateTime.Now.ToString("hh:mm tt")}");
                var pfeclifesavinghub= new pfeclifesavinghub("2023-03-02 11:36:06.150");
                var SavingStocks= await pfeclifesavinghub.GetSavingStock(); 
                if (SavingStocks.Count()>0){
                    foreach (var stock in SavingStocks){
                        var respon= await pfeclifesavinghub.InventoryhubPost(stock);
                        Console.WriteLine(JsonSerializer.Serialize(respon));
                        var dataLog=new {
                            DataRequest= stock,
                            Respon=respon
                        };
                    _logger.LogInformation(JsonSerializer.Serialize(dataLog));
                } 
                }
              
            }

            Console.WriteLine($"Not on Schedule: {JsonSerializer.Serialize(_timeSchedule)}" );
            _logger.LogInformation($"Not on Schedule: {JsonSerializer.Serialize(_timeSchedule)}");
            await Task.Delay(10000);
        }
             
    }
}