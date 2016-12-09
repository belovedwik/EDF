#region using



#endregion

using System.Xml.Serialization;

namespace Databox.Libs.ScePriceUpdate
{
    public class SceProduct
    {
        public int ProductType { get; set; }
        public int ProductId { get; set; }
        public string PartNumber { get; set; }
        public double CostPrice { get; set; }
        public double MSRP { get; set; }
        public double WebPrice { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        [XmlIgnore]
        public bool PriceUpdated { get; set; }
    }
}