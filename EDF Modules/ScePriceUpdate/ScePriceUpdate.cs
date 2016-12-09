#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using Databox.Libs.ScePriceUpdate;
using ScePriceUpdate;
using ScePriceUpdate.DataItems;
using ScePriceUpdate.Extensions;
using ScePriceUpdate.Helpers;

#endregion

namespace WheelsScraper
{
    public class ScePriceUpdate : BaseScraper
    {
        public ScePriceUpdate()
        {
            Name = "ScePriceUpdate";
            Url = "https://www.ScePriceUpdate.com/";
            PageRetriever.Referer = Url;
            WareInfoList = new List<ExtWareInfo>();
            Wares.Clear();
            BrandItemType = 1000;

            SpecialSettings = new ExtSettings();
            Complete += (sender, args) => RemoveFile();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
        }

        private bool _removeFilesAfterUpdate;
        private bool _updateMSRP;
        private bool _updateWebPrice;
        private string _csvName;
        private string _zipName;
        private string _ftpFilesNames;
        private CsvLoader _loader;

        private Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var libName = args.Name.ToLower().Replace('.', '_');
            var p1 = libName.IndexOf(',');
            if (p1 != -1)
                libName = libName.Substring(0, p1);
            if (libName.Contains("_resources"))
                return null;
            Assembly asm = null;

            var rmgr = new ResourceManager("ScePriceUpdate.Libs", typeof (Libs).Assembly);
            rmgr.IgnoreCase = true;

            object obj = rmgr.GetObject(libName);
            var asmBytes = ((byte[]) (obj));
            if (asmBytes != null)
            {
                asm = Assembly.Load(asmBytes);
            }

            return asm;
        }

        private List<SceProduct> LoadSceProducts()
        {
            var products = _loader.LoadSceProducts("SceForPriceUpdateInventory", out _csvName, out _zipName);
            var emtyCostRows = products.Count(p => p.CostPrice == 0);
            if (emtyCostRows > 0)
            {
                MessagePrinter.PrintMessage(
                    string.Format("Detected {0} products with Cost Price = 0, recommended to add cost price manually",
                        emtyCostRows), ImportanceLevel.High);
            }
            return products;
        }

        private List<string> BrandsFromSce()
        {
            return _loader.GetUniqueFieldValues(ExtSett.ProductsFromSce, "Brand");
        }

        private List<string> CategoriesFromSce()
        {
            return _loader.GetUniqueFieldValues(ExtSett.ProductsFromSce, "Category");
        }

        private ExtSettings ExtSett
        {
            get { return (ExtSettings) Settings.SpecialSettings; }
        }

        public override Type[] GetTypesForXmlSerialization()
        {
            return new[] {typeof (ExtSettings)};
        }

        public override Control SettingsTab
        {
            get
            {
                _loader = new CsvLoader(Log, Settings);
                var frm = new ucExtSettings
                {
                    LoadSceProducts = LoadSceProducts,
                    LoadBrands = BrandsFromSce,
                    LoadCategories = CategoriesFromSce,
                    ReadProductsFromFile = ReadProductsFromFile,
                    LoadFTPFilesNames = LoadFtpFilesNames,
                    Sett = Settings
                };
                return frm;
            }
        }

        //TODO: Change UI for select files names
        private List<string> LoadFtpFilesNames()
        {
            MessagePrinter.PrintMessage("Start loading FTP files names");
            var ftpFilesNames = SFTPHelper.LoadFilesList(Settings, ExtSett.FTPPort, ExtSett.FTPWorkingDirectory);
            MessagePrinter.PrintMessage(string.Format("{0} FTP files names loaded", ftpFilesNames.Count));
            return ftpFilesNames;
        }

        private List<SceProduct> ReadProductsFromFile(string fileName)
        {
            var products = _loader.ReadSceProducts(fileName);
            var emtyCostRows = products.Count(p => p.CostPrice == 0);
            if (emtyCostRows > 0)
            {
                MessagePrinter.PrintMessage(
                    string.Format("Detected {0} products with Cost Price = 0, recommended to add cost price manually",
                        emtyCostRows), ImportanceLevel.High);
            }
            return products;
        }

        public override WareInfo WareInfoType
        {
            get { return new ExtWareInfo(); }
        }

        protected override bool Login()
        {
            return true;
        }

        private void RemoveFile()
        {
            if (_removeFilesAfterUpdate)
            {
                if (File.Exists(_csvName))
                {
                    File.Delete(_csvName);
                    MessagePrinter.PrintMessage(string.Format("File {0} removed", _csvName));
                }
                if (File.Exists(_zipName))
                {
                    File.Delete(_zipName);
                    MessagePrinter.PrintMessage(string.Format("File {0} removed", _zipName));
                }
            }
        }

        protected override void RealStartProcess()
        {
            Wares.Clear();
            _removeFilesAfterUpdate = ExtSett.RemoveFilesAfterUpdate;
            _updateMSRP = ExtSett.UpdateMSRP;
            _updateWebPrice = ExtSett.UpdateWebPrice;
            _ftpFilesNames = ExtSett.PriceFiles;
            if (ExtSett.PriceMarkups.Count > 0)
            {
                lstProcessQueue.Add(ExtSett.LoadBeforeUpdate
                    ? new ProcessQueueItem
                    {
                        ItemType = ItemType.LoadProducts
                    }
                    : new ProcessQueueItem
                    {
                        ItemType = ExtSett.LoadPriceFromFTP ? ItemType.UpdatePriceFromFtp : ItemType.UpdatePrice
                    });
            }
            else
            {
                MessagePrinter.PrintMessage(
                    "Price rules list is empty! Please go to Custom->Settings->Price Rules for creating rules",
                    ImportanceLevel.High);
            }

            StartOrPushPropertiesThread();
        }

        private void ProcessLoadProducts(ProcessQueueItem pqi)
        {
            if (cancel)
                return;
            MessagePrinter.PrintMessage("Load products processing");

            try
            {
                ((ucExtSettings) SettingsTab).DoLoadProductsFromSce();

                lstProcessQueue.Add(new ProcessQueueItem
                {
                    ItemType = ExtSett.LoadPriceFromFTP ? ItemType.UpdatePriceFromFtp : ItemType.UpdatePrice
                });
                MessagePrinter.PrintMessage("Load products processed");
            }
            catch (Exception e)
            {
                MessagePrinter.PrintMessage(
                    String.Format("ERROR loading! \n{0}", e.Message), ImportanceLevel.High);
            }

            pqi.Processed = true;
            StartOrPushPropertiesThread();
        }

        private void ProcessUpdatePriceFromFtp(ProcessQueueItem pqi)
        {
            if (cancel)
                return;
            try
            {
                if (ExtSett.ProductsFromSce.Count < 1)
                {
                    throw new ArgumentException(
                        "Products list is empty! Please go to Custom->Settings->Products for loading or switch on checkbox 'Load Before Update'");
                }
                if (string.IsNullOrEmpty(_ftpFilesNames))
                {
                    throw new ArgumentException(
                        "Price files list is empty!");
                }
                var ftpFiles = _ftpFilesNames.Split(',').Select(s=>s.Trim());
                var ftpPriceList = new List<PriceItem>();
                foreach (var ftpFile in ftpFiles)
                {
                    MessagePrinter.PrintMessage(string.Format("Loading FTP file: {0}", ftpFile));
                    var priceFile = SFTPHelper.DownloadFile(Settings, ExtSett.FTPPort, ExtSett.FTPWorkingDirectory, ftpFile);
                    MessagePrinter.PrintMessage(string.Format("File loaded {0}", priceFile));
                    var priceList = FileHelper.ReadPriceItems(priceFile);
                    if (ftpPriceList.Any())
                    {
                        MessagePrinter.PrintMessage("Checking duplicates...");
                        foreach (var priceItem in from priceItem in priceList
                            let itemExist = ftpPriceList.FirstOrDefault(i => i.PartNumber == priceItem.PartNumber)
                            where itemExist == null
                            select priceItem)
                        {
                            ftpPriceList.Add(priceItem);
                        }
                    }
                    else
                    {
                        ftpPriceList.AddRange(priceList);
                    }
                }
                MessagePrinter.PrintMessage(string.Format("{0} price items loaded", ftpPriceList.Count));
                MessagePrinter.PrintMessage("Updating price from FTP...");
                foreach (var product in ModuleSettings.Default.ProductsFromSce)
                {
                    var priceItem = ftpPriceList.FirstOrDefault(p => p.PartNumber == product.PartNumber);
                    if (priceItem != null)
                    {
                        product.CostPrice = priceItem.Price;
                        product.PriceUpdated = true;
                    }
                    else
                    {
                        product.PriceUpdated = false;
                    }
                }

                MessagePrinter.PrintMessage(string.Format("{0} products price updated\n{1} part numbers not found",
                    ModuleSettings.Default.ProductsFromSce.Count(p => p.PriceUpdated),
                    ModuleSettings.Default.ProductsFromSce.Count(p => !p.PriceUpdated)));
                ((ucExtSettings)SettingsTab).RefreshBindings();

                lstProcessQueue.Add(new ProcessQueueItem
                {
                    ItemType = ItemType.UpdatePrice
                });
            }
            catch (Exception e)
            {
                MessagePrinter.PrintMessage(
                    String.Format("ERROR updating price from FTP! \n{0}", e.Message), ImportanceLevel.High);
            }

            pqi.Processed = true;
            StartOrPushPropertiesThread();
        }

        private void ProcessUpdatePrice(ProcessQueueItem pqi)
        {
            if (cancel)
                return;
            MessagePrinter.PrintMessage("Updating price from Cost...");
            try
            {
                if (ExtSett.ProductsFromSce.Count < 1)
                {
                    throw new ArgumentException(
                        "Products list is empty! Please go to Custom->Settings->Products for loading or switch on checkbox 'Load Before Update'");
                }
                var productList = ExtSett.ProductsFromSce;
                if (ExtSett.UseBrands)
                {
                    productList = productList.Where(p => ExtSett.SelectedBrands.Contains(p.Brand)).ToList();
                }
                if (ExtSett.UseCategories)
                {
                    productList = productList.Where(p => ExtSett.SelectedCategories.Contains(p.Category)).ToList();
                }
                if (ExtSett.LoadPriceFromFTP)
                {
                    productList = productList.Where(p => p.PriceUpdated).ToList();
                }
                foreach (var product in productList)
                {
                    var markUp = product.CostPrice.CountMurkup(ExtSett.PriceMarkups);
                    var wi = product.Product2ExtWareinfo();
                    if (_updateMSRP)
                    {
                        wi.MSRP = markUp;
                    }
                    if (_updateWebPrice)
                    {
                        wi.WebPrice = markUp;
                    }
                    AddWareInfo(wi);
                    OnItemLoaded(wi);
                    product.PriceUpdated = true;
                }
                MessagePrinter.PrintMessage(string.Format("{0} products price updated", productList.Count));
            }
            catch (Exception e)
            {
                MessagePrinter.PrintMessage(
                    String.Format("ERROR updating! \n{0}", e.Message), ImportanceLevel.High);
            }

            pqi.Processed = true;
            StartOrPushPropertiesThread();
        }

        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;
            switch (item.ItemType)
            {
                case ItemType.LoadProducts:
                    act = ProcessLoadProducts;
                    break;
                case ItemType.UpdatePriceFromFtp:
                    act = ProcessUpdatePriceFromFtp;
                    break;
                case ItemType.UpdatePrice:
                    act = ProcessUpdatePrice;
                    break;
                default:
                    act = null;
                    break;
            }
            return act;
        }
    }
}
