using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WheelsScraper;

namespace Zeiglers
{
    public class ExtWareInfo : WareInfo
    {
        public string ProductTitle { get; set; }
        public string ProductDescription { get; set; }
        public string Weight { get; set; }
		public string Price_string { get; set; }
        public string ImagesList { get; set; }
    }
}
