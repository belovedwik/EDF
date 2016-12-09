#region using

using Databox.Libs.ScePriceUpdate;

#endregion

namespace ScePriceUpdate.Extensions
{
     public static class SceProductExtension
     {
          public static ExtWareInfo Product2ExtWareinfo(this SceProduct sceProduct)
          {
               return new ExtWareInfo
                           {
                                ProductType = sceProduct.ProductType,
                                ProductId = sceProduct.ProductId,
                                PartNumber = sceProduct.PartNumber,
                                CostPrice = sceProduct.CostPrice,
                                WebPrice = sceProduct.WebPrice,
                                MSRP = sceProduct.MSRP
                           };
          }
     }
}
