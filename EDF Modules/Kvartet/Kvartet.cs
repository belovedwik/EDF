using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using Kvartet;
using Databox.Libs.Kvartet;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Xml.Linq;
using System.Xml;

namespace WheelsScraper
{
    public class Kvartet : BaseScraper
    {
        private string _login = "FUBAOMING";
        private string _pwd = "scvcok9va";
        public Kvartet()
        {
            Name = "Kvartet";
            Url = "https://www.mahoneswallpapershop.com";
            PageRetriever.Referer = Url;
            WareInfoList = new List<ExtWareInfo>();
            Wares.Clear();
            BrandItemType = 2;

            SpecialSettings = new ExtSettings();
        }

        private ExtSettings extSett
        {
            get
            {
                return (ExtSettings)Settings.SpecialSettings;
            }
        }

        public override Type[] GetTypesForXmlSerialization()
        {
            return new Type[] { typeof(ExtSettings) };
        }

        //public override System.Windows.Forms.Control SettingsTab
        //{
        //	get
        //	{
        //		var frm = new ucExtSettings();
        //		frm.Sett = Settings;
        //		return frm;
        //	}
        //}

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
            List<ExtWareInfo> listProductFile = GetItemsFromFile();

            foreach (var itemProductFile in listProductFile)
            {
                lstProcessQueue.Add(new ProcessQueueItem { ItemType = 1, Item = itemProductFile });
            }
            StartOrPushPropertiesThread();
        }

        private string[] GetFileList()
        {
            var localPath = Settings.Location;
            var csvPath = localPath.Substring(0, localPath.Length - (localPath.Length - localPath.LastIndexOf('\\')) + 1) + "\\KvartetData\\";

            return Directory.GetFiles(csvPath, "*.csv");
        }

        private List<ExtWareInfo> GetItemsFromFile()
        {
            List<ExtWareInfo> Items = new List<ExtWareInfo>();

            int i = 0;    
            foreach (string FielePath in GetFileList())
            { 
                using (var sr = File.OpenText(FielePath))
                {
                    using (var csv = new CsvReader(sr, true, ','))
                    {
                        var headers = csv.GetFieldHeaders();
                        while (csv.ReadNextRecord())
                        {
                            i++;
                            if (cancel)
                                break;

                            ExtWareInfo wi = new ExtWareInfo();

                            wi.primaryKeyword = csv["primary keyword"];
                            wi.ProductTitle = csv["Product Title"];

                          //  if (!string.IsNullOrEmpty(wi.primaryKeyword))
                            {
                                var patternGeneral = correctPartNumber(wi.primaryKeyword.Trim('@'));

                                string _pattern = patternGeneral.Substring(0, patternGeneral.LastIndexOf("-"));
                                string _color = patternGeneral.Substring(patternGeneral.LastIndexOf("-"));

                                var checkUrl = string.Format("http://www.e-designtrade.com/api/stock_check.asp?user={0}&password={1}&pattern={2}&color={3}&identifier=0&quantity=1", _login, _pwd, _pattern, _color);
                                var xmlResp = PageRetriever.ReadFromServer(checkUrl, true);

                                var xDoc = new XmlDocument();
                                xDoc.LoadXml(xmlResp);
                                var status = xDoc.SelectSingleNode("//TRANSACTION_STATUS");
                                if (status.InnerText.StartsWith("Invalid"))
                                {
                                    MessagePrinter.PrintMessage("Invalid product:" + i + " " + wi.ProductTitle);    
                                    continue;
                                }
                                   
                            }
                            
                            /* wi.PartNumberMidified = correctPartNumber(wi.PartNumber);
                        */
                            wi.BulletPoint = csv["Bullet Point"];
                            wi.Prodid = csv["Prodid"];
                           
                            wi.ProductType = csv["Product Type"];
                            
                            wi.AnchorText = csv["Anchor Text"];
                        
                            wi.AmazonDescription = csv["Amazon Description"];
                            wi.SubTitle = csv["Sub Title"];
                            wi.AdditionalDescription = csv["Additional Description"];
                            wi.AdditionalDescription2 = csv["Additional Description 2"];
                            wi.AdditionalDescription3 = csv["Additional Description 3"];
                            wi.AdditionalDescription4 = csv["Additional Description 4"];
            
                            wi.secondaryKeyword = csv["secondary keyword"];
                            wi.tertiaryKeyword = csv["tertiary keyword"];
                            wi.GeneralImageTags = csv["General Image Tags"];
                            wi.GeneralImage = csv["General Image"];
                            wi.upsellProdQty = csv["upsellProdQty"];
                         
                            Items.Add(wi);

                            OnItemLoaded(wi);
                            AddWareInfo(wi);
                            StartOrPushPropertiesThread();
                        }
                    }
                }
            }

            return Items;
        }

        private string correctPartNumber(string p)
        {
            if (p.Contains("-"))
            {
                return p;
            }

            while (p.Length < 7)
            {
                p = "0" + p;
            }

            p = p.Insert(p.Length - 4, "-");

            return p;
        }

        protected void ProcessBrandsListPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return;

            pqi.Processed = true;
            MessagePrinter.PrintMessage("Brands list processed");
            StartOrPushPropertiesThread();
        }

        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;
            if (item.ItemType == 1)
                act = ProcessBrandsListPage;
            else act = null;

            return act;
        }
    }
}
