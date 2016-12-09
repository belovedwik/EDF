#region using

using System.Collections.Generic;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using ScePriceUpdate.DataItems;
using ScePriceUpdate.Extensions;

#endregion

namespace ScePriceUpdate.Helpers
{
    public static class FileHelper
    {
        public static List<PriceItem> ReadPriceItems(string fileName)
        {
            try
            {
                var products = new List<PriceItem>();
                using (var sr = File.OpenText(fileName))
                {
                    using (var csv = new CsvReader(sr, true, ','))
                    {
                        csv.GetFieldHeaders();

                        while (csv.ReadNextRecord())
                        {
                            var partNum = csv["part_number"];
                            var curProduct = new PriceItem
                            {
                                PartNumber = partNum.Trim(),
                                Price = csv["price"].ParsePrice()
                            };
                            products.Add(curProduct);
                        }
                    }
                }
                return products;
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }
    }
}
