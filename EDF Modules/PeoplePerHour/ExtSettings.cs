using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Databox.Libs.PeoplePerHour
{
    public class ExtSettings
    {

        public List<Tags> TagList { get; set; }
    }

    public class Tags {
        public string SearchTag { get; set; }
        public string SearchKeyword { get; set; }
    }
}
