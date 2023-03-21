using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Pfizer.hub.job.DTO;

namespace Pfizer.hub.job
{
    public class pfeclifesavinghub
    {
       PfizerstockLoginResponDTO loginRespon;
       private string UpdateDate;
        private readonly HttpClient client = new HttpClient();
        private string? Token=null;
        private string LoginUrl="https://localhost:5001/api/Account/login";
        private string StockFeederUrl="https://localhost:5001/api/StockFeeder/SendStockInformation";
        private string PostStockInfoUrl= "https://localhost:5001/api/LifeSavingHub/PostStockInfo";
        private dynamic Account= new {
            Username="clientusr",
            Password="Aplprim4$1234567890"
        };

        public pfeclifesavinghub(string UpdateDate){
          this.UpdateDate=UpdateDate;
        }


      public async Task<PfizerstockLoginResponDTO> GetAuthorization () {
           StringContent jsonContent = new(
                JsonSerializer.Serialize(Account),
                Encoding.UTF8,
                "application/json");
            var  loginRespon= new PfizerstockLoginResponDTO();
            var  responseMessage= new HttpResponseMessage();
            while(!loginRespon.Success){
                  try
                  {
                    responseMessage = await client.PostAsync(LoginUrl, jsonContent);
                    var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                    loginRespon= JsonSerializer.Deserialize<PfizerstockLoginResponDTO>(jsonResponse);
                    loginRespon.Success=responseMessage.StatusCode.Equals(HttpStatusCode.OK);
                }catch (Exception err){
                  await Task.Delay(1000);
                  Console.WriteLine(err.Message);
                }
            }

         return  loginRespon;
        }
    
      public async Task<IEnumerable<LifeSavingStockDTO>> GetSavingStock(){
        if (Token==null){
          var Authorization=await GetAuthorization();
          Token= String.Concat("Bearer ", Authorization.Data.Token);
        }        
         client.DefaultRequestHeaders.Add("Authorization", Token);
         var param= new Dictionary<string, string>{
            ["getDataByDate"]=this.UpdateDate.ToString()
         };
         var query= QueryHelpers.AddQueryString(StockFeederUrl, param);
         var LifeSavingStock= new List<LifeSavingStockDTO>();
         while(LifeSavingStock.Count<=0){
             try{
                var responseMessage= await client.PostAsync(query, null);
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();
                var ObjRespon= JsonSerializer.Deserialize<SendStockInformationResponDTO>(jsonResponse);
                LifeSavingStock=ObjRespon.Data.ToList();
              }catch (Exception err){
                  Console.WriteLine(err.Message);
                  await Task.Delay(500000);
              }

         }
        
         return LifeSavingStock;
      }

      public async Task<InventoryhubResponDTO> InventoryhubPost(LifeSavingStockDTO Productinput)
      {
         StringContent jsonContent = new(
                JsonSerializer.Serialize(Productinput),
                Encoding.UTF8,
                "application/json");
        var lifeSavingStocks= GetSavingStock();
        if (Token==null){
          var Authorization=await GetAuthorization();
          Token= String.Concat("Bearer ", Authorization.Data.Token);
        };
        var param= new Dictionary<string, string>{
          ["DateUpdate"]= this.UpdateDate.ToString()
        };

        var query= QueryHelpers.AddQueryString(PostStockInfoUrl, param);
        var Respon= new InventoryhubResponDTO();
        try{
            var responseMessage= await client.PostAsync(query, jsonContent);
            Console.WriteLine($"Status: {responseMessage.StatusCode}");
            var SResponse= await responseMessage.Content.ReadAsStringAsync();
            
            Respon= JsonSerializer.Deserialize<InventoryhubResponDTO>(SResponse);
        }catch(Exception err){
            throw err;
        }
        return Respon;
      }
}
    
}
