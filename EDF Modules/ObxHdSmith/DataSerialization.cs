using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObxHdSmith
{
    public class DataSerialization
    {
        public string ResultsHeaderHtml { get; set; }
        public string ResultsHtml { get; set; }
        public Facets Facets { get; set; }
        public bool LoadFacets { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Facets
    {
        public string FacetsHtml { get; set; }
        public string BreadcrumbHtml { get; set; }
        public int FacetCount { get; set; }

    }
    public class Pagination
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalResults { get; set; }
        public int TotalPages { get; set; }
        public bool HasMultiplePages { get; set; }
    }

}
