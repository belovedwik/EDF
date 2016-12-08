using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WheelsScraper;

namespace PeoplePerHour
{
    public class ExtWareInfo : WareInfo
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        //public string Name { get; set; }
        public string Country { get; set; }
        public string Link { get; set; }
        public string ProfileLink { get; set; }
        public string Delivery { get; set; }

        // Page2
        public int Sales { get; set; }
        public int Views { get; set; }
        public int Favorites { get; set; }
        public string ReviewRating { get; set; }
        public string Responce { get; set; }
        public string Review { get; set; }
    }
}
