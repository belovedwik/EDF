using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WheelsScraper;

namespace Eswimelite
{
    public class ExtWareInfo : WareInfo
    {
        public string Action { get; set; }
        public int ProductType { get; set; }
        public string ProductTitle { get; set; }
        public string BulletPoint { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MainCategory { get; set; }
        public string SubCategory { get; set; }
        public string Section { get; set; }

        public double WebPrice { get; set; }
        public string ImagesList { get; set; }

        public string Specification { get; set; }
        public string PrimaryOptionTitle { get; set; }
        public string PrimaryOptionChoice { get; set; }
    }
}
