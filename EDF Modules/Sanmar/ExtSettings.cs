using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Databox.Libs.Sanmar
{
    public class ExtSettings
    {
        public int FTPPort { get; set; }
        public string FTPWorkingDirectory { get; set; }

        public DateTime DateFrom { get; set; } 

        public DateTime DateTo { get; set; }

        [XmlIgnore]
        public List<SceProduct> ProductsFromSce
        {
            get { return ModuleSettings.Default.ProductsFromSce; }
        }

    }

    public class SceProduct
    {
  
        public string customerNum { get; set; }
        public string invoiceNum { get; set; }
     
    }

    public class ModuleSettings
    {
        private static ModuleSettings _default;

        public static ModuleSettings Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new ModuleSettings();
                }
                return _default;
            }
        }
        public ModuleSettings()
        {
            ProductsFromSce = new List<SceProduct>();
        }
        public List<SceProduct> ProductsFromSce { get; set; }

    }
}
