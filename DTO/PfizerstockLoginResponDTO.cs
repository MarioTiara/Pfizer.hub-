using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pfizer.hub.job.DTO
{
    public class PfizerstockLoginResponDTO
    {
        public bool Success  {get;set;}= false;
        public string? Message {get;set;}
        public Data Data {get;set;}
    }

    public class Data {
       public string? Username {get;set;}
       public string? Token {get;set;}
    } 
}