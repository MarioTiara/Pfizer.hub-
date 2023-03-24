using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Pfizer.hub.job.DTO;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using System.Net;

namespace Pfizer.hub.job
{
    public class PizerStock
    {
        private string _url;
        private readonly ILogger _logger;
        private string _bearerToken ;
        public PizerStock (IConfiguration configuration, IServiceProvider services, string Token){
            _url=configuration.GetValue<string>("url:StockFeederUrl");
            _logger=services.GetService<ILogger<PizerStock>>();
            _bearerToken= Token;
        }
        public async Task<Queue<LifeSavingStockDTO>> GetSavingStock(string DateUpdate){
         HttpClientHandler clientHandler = new HttpClientHandler();
         clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
         HttpClient client = new HttpClient(clientHandler);
         
         client.DefaultRequestHeaders.Add("Authorization", _bearerToken);
         var param= new Dictionary<string, string>{
            ["getDataByDate"]=DateUpdate
         };
         var query= QueryHelpers.AddQueryString(_url, param);
         
         var stocks= new Queue<LifeSavingStockDTO>();
         while(stocks.Count<=0){
             try{
                var responseMessage= await client.PostAsync(query, null);
                if (responseMessage.StatusCode==HttpStatusCode.OK){
                    var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                    if (jsonResponse!=null){
                        var ObjRespon= JsonSerializer.Deserialize<SendStockInformationResponDTO>(jsonResponse);
                        foreach (var stock in ObjRespon.Data) stocks.Enqueue(stock);
                    }
                }
                
              }catch (Exception err){
                  _logger.LogError(err, err.Message);
                  await Task.Delay(10000);
              }

         }
         return stocks;
      }


    }
}