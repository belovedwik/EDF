using System;
using System . Collections . Generic;
using System . Text;
using WheelsScraper;

namespace ScePriceUpdate
{
     public class ExtWareInfo : WareInfo
     {
          public int ProductType { get; set; }
          public int ProductId { get; set; }
          public string Action { get; set; }
          public double CostPrice { get; set; }
          public double WebPrice { get; set; }

          public ExtWareInfo()
          {
               Action = "update";
          }
     }
}
