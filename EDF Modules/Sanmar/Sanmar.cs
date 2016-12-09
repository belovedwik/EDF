using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using Sanmar;
using Databox.Libs.Sanmar;
using Sanmar.Helper;
using Renci.SshNet;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace WheelsScraper
{
    public class Sanmar : BaseScraper
    {
        public Sanmar()
        {
            Name = "Sanmar";
            Url = "https://www.Sanmar.com/";
            PageRetriever.Referer = Url;
            WareInfoList = new List<ExtWareInfo>();
            Wares.Clear();
            BrandItemType = 2;

            //ExtSett.DateFrom = DateTime.Now.Date;
            //ExtSett.DateTo = DateTime.Now.Date;

            SpecialSettings = new ExtSettings();

        }


        public override Type[] GetTypesForXmlSerialization()
        {
            return new Type[] { typeof(ExtSettings) };
        }

        public override System.Windows.Forms.Control SettingsTab
        {
            get
            {
                var frm = new ucExtSettings();
                frm.Sett = Settings;
                return frm;
            }
        }

        public override WareInfo WareInfoType
        {
            get
            {
                return new ExtWareInfo();
            }
        }

        protected override bool Login()
        {
            return true;
        }

        protected override void RealStartProcess()
        {
      

            lstProcessQueue.Add(new ProcessQueueItem { URL = Url, ItemType = 1 });
            StartOrPushPropertiesThread();
        }

        private ExtSettings ExtSett
        {
            get { return (ExtSettings)Settings.SpecialSettings; }
        }

        protected void ProcessFTPFileList(ProcessQueueItem pqi)
        {
            
            if (cancel)
                return;

            char separator = '\t';

            List<KeyValuePair<string, string>> OrderDict = new List<KeyValuePair<string, string>>();

            // загружаем список файлов с ФТП, и фильтруем по дате
            var FTPFileList = SFTPHelper.LoadFilesList(Settings, ExtSett);

            // грузим файлы и получаем данные с них
            if (FTPFileList.Count() > 0) {
                foreach (string file in FTPFileList)
                {
                    string tmpFile = SFTPHelper.DownloadFile(Settings, ExtSett, file);
                    using (var sr = File.OpenText(tmpFile))
                    {
                        using (var csv = new CsvReader(sr, true, separator))
                        {
                            //csv.GetFieldHeaders();
                            while (csv.ReadNextRecord())
                            {
                                var invoice = csv["INVOICE#"];
                                var customerNum = csv["CUSTOMER PO"];
                                if (!OrderDict.Any(k => k.Key == invoice))
                                    OrderDict.Add(new KeyValuePair<string, string>(invoice, customerNum));
                            }
                        }
                    }
                }
            }
            MessagePrinter.PrintMessage( string.Format("FTP files loaded, {0} items", OrderDict.Count()) );

            int _updatedProd = 0; 
            foreach (var order in OrderDict) {
                var product = ModuleSettings.Default.ProductsFromSce.FirstOrDefault(p=>p.customerNum == order.Key);
                if (product != null)
                {
                    product.customerNum = order.Value;
                    _updatedProd++;

                    var wi = product.Product2ExtWareinfo();
                    AddWareInfo(wi);

                }
            }
            MessagePrinter.PrintMessage(string.Format("{0} products updates", _updatedProd));

            pqi.Processed = true;
            StartOrPushPropertiesThread();
        }

        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;
            if (item.ItemType == 1)
                act = ProcessFTPFileList;
            else act = null;

            return act;
        }
    }
}
