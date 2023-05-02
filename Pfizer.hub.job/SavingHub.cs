using System.Net;
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
        private string _url;
        private readonly ILogger _logger;
        private string _bearerToken ;
        public SavingHub (IConfiguration configuration, IServiceProvider services, string Token){
            _url=configuration.GetValue<string>("url:PostStockInfoUrl");
            _logger=services.GetService<ILogger<PizerStock>>();
            _bearerToken= Token;
        }

       public async Task SendStocks(Queue<LifeSavingStockDTO> stocks, string DateUpdate,  bool production=true){
            HttpClientHandler clientHandler = new HttpClientHandler();
            if (production!=true){
                 clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            }
            HttpClient client = new HttpClient(clientHandler);
            client.DefaultRequestHeaders.Add("Authorization", _bearerToken);
            while (stocks.Count>0){
                try{
                    var product= stocks.Dequeue();
                    var content= BuildContentBody(product);
                    var responseMessage= await client.PostAsync(_url, content);

                    _logger.LogInformation($"SavingHUb - Status Code: {responseMessage.StatusCode}");

                    if (responseMessage.StatusCode==HttpStatusCode.OK){
                        var SResponse= await responseMessage.Content.ReadAsStringAsync();
                        InventoryhubResponDTO? responDTO = new InventoryhubResponDTO();
                        responDTO= JsonSerializer.Deserialize<InventoryhubResponDTO>(SResponse);
                        var dataLog= new {DataRequest=product, Respon=responDTO};
                        _logger.LogInformation(JsonSerializer.Serialize(dataLog));
                    }else{
                         _logger.LogWarning($"Failed to post product from API, Respon: {responseMessage.StatusCode}");
                         await Task.Delay(1000);
                    }
                }catch (Exception err){
                    _logger.LogError(err, err.Message);
                    await Task.Delay(1000);
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