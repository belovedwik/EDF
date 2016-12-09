#region using

using System.Collections.Generic;
using System.Xml.Serialization;

#endregion

namespace Databox.Libs.ScePriceUpdate
{
    public class ExtSettings

    {
        public ExtSettings()
        {
            SelectedCategories = new List<string>();
            SelectedBrands = new List<string>();
        }

        public bool LoadBeforeUpdate { get; set; }

        public bool RemoveFilesAfterUpdate { get; set; }

        public bool UseBrands { get; set; }

        public bool UseCategories { get; set; }

        public bool UpdateMSRP { get; set; }

        public bool UpdateWebPrice { get; set; }

        [XmlIgnore]
        public List<SceProduct> ProductsFromSce
        {
            get { return ModuleSettings.Default.ProductsFromSce; }
        }

        [XmlIgnore]
        public List<PriceMarkup> PriceMarkups
        {
            get { return ModuleSettings.Default.PriceMarkups; }
        }

        public bool LoadPriceFromFTP { get; set; }
        public string PriceFiles { get; set; }
        public int FTPPort { get; set; }
        public string FTPWorkingDirectory { get; set; }

        [XmlIgnore]
        public List<string> SelectedBrands { get; set; }

        [XmlIgnore]
        public List<string> SelectedCategories { get; set; }
    }
}
