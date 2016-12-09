using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WheelsScraper;

namespace Kvartet
{
    public class ExtWareInfo : WareInfo
    {
        public string Prodid { get; set; }
        public string ProductType { get; set; }
        public string MerchandiseType { get; set; }
        public string ProductTitle { get; set; }
        public string AnchorText { get; set; }

        
        public string AmazonTitle { get; set; }
        public string AmazonDescription { get; set; }

        public string SubTitle { get; set; }

        public string AdditionalDescription{ get; set; }
        public string AdditionalDescription2 { get; set; }
        public string AdditionalDescription3 { get; set; }
        public string AdditionalDescription4 { get; set; }
        public string BulletPoint { get; set; }
        public string primaryKeyword { get; set; }
        public string secondaryKeyword { get; set; }
        public string tertiaryKeyword { get; set; }
        public string GeneralImageTags{ get; set; }
        public string GeneralImage{ get; set; }
        public string upsellProdQty{ get; set; }


    }
}
