using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pfizer.hub.job.DTO
{
     public class Branch
    {
        public string branchName { get; set; }
        public string city { get; set; }
        public List<Detail> details { get; set; } = new List<Detail>();
    }

    public class Detail
    {
        public string dossageForm { get; set; }
        public int qty { get; set; }
    }

    public class Products
    {
        public string DateUpdate {get;set;}
        public string distributor { get; set; }
        public string productName { get; set; }
        public List<Branch> branches { get; set; } = new List<Branch>();

    }

    

    public class LifeSavingStockDTO
    {
        public Products products { get; set; }
        public int timestamp { get; set; }
        public string checksum { get; set; }
    }

}