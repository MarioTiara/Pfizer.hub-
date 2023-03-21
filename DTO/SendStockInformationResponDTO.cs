using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pfizer.hub.job.DTO
{
    public class SendStockInformationResponDTO
    {
        public IEnumerable<LifeSavingStockDTO>? Data {get;set;}
        public string? Message {get;set;}
    }
}