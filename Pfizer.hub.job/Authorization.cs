using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pfizer.hub.job.DTO;

namespace Pfizer.hub.job
{
    public class Authorization
    {
       
        private string Token {get;set;}
        private PizerAccount account= new PizerAccount(); 
        private readonly ILogger _logger;
        private string _loginUrl;
        public Authorization (IConfiguration configuration, IServiceProvider services){
            account.Username= configuration.GetValue<string>("pizeraccount:Username");
            account.Password=configuration.GetValue<string>("pizeraccount:Password");
            _loginUrl=configuration.GetValue<string>("url:LoginUrl");
             _logger=services.GetService<ILogger<Authorization>>();
        }

        public async Task Authorize () {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient client = new HttpClient(clientHandler);
            
           StringContent jsonContent = new(
                JsonSerializer.Serialize(account),
                Encoding.UTF8,
                "application/json");
            var  loginRespon= new PfizerstockLoginResponDTO();
            var  responseMessage= new HttpResponseMessage();
            while(!loginRespon.Success){
                  try
                  {
                    responseMessage = await client.PostAsync(_loginUrl, jsonContent);
                    var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                    loginRespon= JsonSerializer.Deserialize<PfizerstockLoginResponDTO>(jsonResponse);
                    loginRespon.Success=responseMessage.StatusCode.Equals(HttpStatusCode.OK);
                    var logger =JsonSerializer.Serialize(new {
                        statusCode=responseMessage.StatusCode,
                        Content=loginRespon
                    });

                    _logger.LogInformation($"Login Request -{logger} ");
                }catch (Exception err){
                  await Task.Delay(1000);
                  _logger.LogError(err, err.Message);
                }
            }

           this.Token=loginRespon.Data.Token;
        }

        public async Task<string> GetBeareToken(){
            if (this.Token is null) await Authorize();
            return String.Concat("Bearer ", this.Token);
        }
    }
}