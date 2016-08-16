using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WheelsScraper;

namespace Zeiglers
{
    public class ExtWareInfo : WareInfo, ICloneable
    {
        public string Action { get; set; }
        public int ProductType { get; set; }
        public string BulletPoint { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MainCategory { get; set; }
        public string SubCategory { get; set; }
        public string Section { get; set; }

        public double WebPrice { get; set; }
        public string ProductTitle { get; set; }
        public string ProductDescription { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double ShippingWeight { get; set; }
        public double ShippingHeight { get; set; }
        public double ShippingWidth { get; set; }
        public double ShippingLength { get; set; }

        public string PrimaryOptionTitle { get; set; }
        public string PrimaryOptionChoice { get; set; }
        public string SecondaryOptionTitle { get; set; }
        public string ScondaryOptionChoice { get; set; }
        public string ProdID { get; set; }
        public string ImagesList { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
