using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BackLinks;

namespace Databox.Libs.BackLinks
{
    public class ExtSettings
    {
        public List<Category> CategotySearchList { get; set; }

        public List<string> CategotyFoundList { get; set; }
    }

    public class Category
    {
        public string CategoryName { get; set; }
    }


}
