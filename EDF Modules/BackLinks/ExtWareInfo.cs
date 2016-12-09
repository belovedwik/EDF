using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WheelsScraper;

namespace BackLinks
{
    public class ExtWareInfo : WareInfo
    {
        public string id { get; set; }
        public string Title { get; set; }
        public string LandingPage { get; set; }
        public string PriceText { get; set; }
        public decimal? Price { get; set; }

        // more info
        public string Moz { get; set; }
        public string Backlinks { get; set; }
        public string DomainAge { get; set; }
        public string MozRank { get; set; }
        public string Alexa { get; set; }
        public string WebSiteAdress { get; set; }
  
    }
}
