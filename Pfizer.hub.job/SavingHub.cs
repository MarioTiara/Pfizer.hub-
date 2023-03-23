using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pfizer.hub.job.DTO;

namespace Pfizer.hub.job
{
    public class SavingHub
    {
        private HttpClient client= new HttpClient();
        private string _url;
        private readonly ILogger _logger;
        private string _bearerToken ;
        public SavingHub (IConfiguration configuration, IServiceProvider services, string Token){
            _url=configuration.GetValue<string>("url:PostStockInfoUrl");
            _logger=services.GetService<ILogger<PizerStock>>();
            _bearerToken= Token;
        }

       public async Task SendStocks(Queue<LifeSavingStockDTO> stocks, string DateUpdate){
            client.DefaultRequestHeaders.Add("Authorization", _bearerToken);
            var param= new Dictionary<string, string>{
            ["DateUpdate"]= DateUpdate
            };
            var query= QueryHelpers.AddQueryString(_url, param);
            while (stocks.Count>0){
                try{
                    var product= stocks.Dequeue();
                    var content= BuildContentBody(product);
                    var responseMessage= await client.PostAsync(query, content);
                    var SResponse= await responseMessage.Content.ReadAsStringAsync();
                    if (SResponse!=null){
                        var responDTO= JsonSerializer.Deserialize<InventoryhubResponDTO>(SResponse);
                        var dataLog= new {DataRequest=product, Respon=responDTO};
                        _logger.LogInformation(JsonSerializer.Serialize(dataLog));
                    }
                }catch (Exception err){
                    _logger.LogError(err, err.Message);
                }
                
            }

       }

       private StringContent BuildContentBody(LifeSavingStockDTO product){
            StringContent jsonContent = new(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");

            return jsonContent;
       }
    }
}