using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using ObxHdSmith;
using Databox.Libs.ObxHdSmith;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace WheelsScraper
{
    public class ObxHdSmith : BaseScraper
    {
        public ObxHdSmith()
        {
            Name = "ObxHdSmith";
            Url = "https://www.ObxHdSmith.com/";
            PageRetriever.Referer = Url;
            WareInfoList = new List<ExtWareInfo>();
            Wares.Clear();
            BrandItemType = 1000;

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
            MessagePrinter.PrintMessage("Starting login");
            var login = GetLoginInfo();
            if (login == null)
                throw new Exception("No valid login found");

            string loginURL = "https://api.hdsmith.com/obx/v1/auth/login?username=" + login.Login + "&password=" + login.Password + "&clientId=obx_mp";
            string data = "username=" + login.Login + "&password=" + login.Password + "&clientId=obx_mp";
            PageRetriever.Referer = "https://ob.hdsmith.com/apps/home/";
            var html = PageRetriever.WriteToServer(loginURL, data, true, true);

            html = PageRetriever.ReadFromServer("https://obx.hdsmith.com/home/welcome/home", true);

            if (html.Contains("Logout"))
            {
                MessagePrinter.PrintMessage("Login done.");
                return true;
            }

            return false;
        }

        private static string accountId;
        protected override void RealStartProcess()
        {
            List<ExtWareInfo> listProductFile = GetItemsNumberFromFile();
            var html = PageRetriever.ReadFromServer("https://obx.hdsmith.com/OrderBaseXpressCS/Hhc", true);
            var htmlDoc = CreateDoc(html);
            accountId = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='selected-account-id']").AttributeOrNull("value");
            foreach (var itemProductFile in listProductFile)
            {
                lstProcessQueue.Add(new ProcessQueueItem { ItemType = 1, Item = itemProductFile });
            }
            StartOrPushPropertiesThread();
        }

        protected void ProcessBrandsListPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return;
            ExtWareInfo wi = (ExtWareInfo)pqi.Item;
            string  partNumber = wi.PartNumberMidified;
            try
            {
                PageRetriever.ContentType = "text/plain";
                var data1 = "searchText=" + partNumber + "\r\nisPaging=false\r\nsearchType=8\r\nsearchSort=RowId\r\nsearchPageSize=25\r\nsearchCurrentPage=1\r\nsearchFilter=\r\nsynonym=false\r\nshowNonStockedItems=false\r\nsAdvancedSearch=||0|0";
                var html = PageRetriever.WriteToServer("https://obx.hdsmith.com/OrderBaseXpressCS/ajax/OrderBaseXpressCS.Forms.Orders.Orders,OrderBaseXpressCS.ashx?_method=GetSearchData&_session=rw", data1, true);

                foreach (var item in html.Split('|'))
                {
                    if (item.Trim().StartsWith("<table"))
                    {
                        html = item.Replace(@"\'", "'");
                        break;
                    }
                }
                bool foundProduct = false;
                var htmlDoc = CreateDoc(html);
                var tagProduct = htmlDoc.DocumentNode.SelectNodes("//table/tbody/tr/td[@title='Item Number']/a");
                if (tagProduct != null)
                {
                    
                    foreach (var itemProduct in tagProduct)
                    {
                        string itemPartnumber = itemProduct.InnerTextOrNull();
                        if (itemPartnumber != null)
                            itemPartnumber = itemPartnumber.Replace("-", "").Replace("Item #:", "").Trim();
                        if (itemPartnumber.ToLower() == partNumber.ToLower().Replace("-", "").Trim())
                        {
                            string link="";
                            var matchesParameterUrl = Regex.Matches(itemProduct.AttributeOrNull("onclick"),@"'(\d+?)'");
                            if (matchesParameterUrl != null && matchesParameterUrl.Count==2)
                            {
                                link =string.Format("https://obx.hdsmith.com/OrderBaseXpress/Application%20Forms/Common/itemICHV.aspx?id={0}&accountId={1}",
                                                        matchesParameterUrl[0].Groups[1].Value,matchesParameterUrl[1].Groups[1].Value);
                            }
                            if (link != null)
                            {
                                lock (this)
                                {
                                    foundProduct = true;
                                    lstProcessQueue.Add(new ProcessQueueItem { ItemType = 2, URL = link, Item=wi });
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!foundProduct)
                {
                    AddWareInfo(wi);
                    MessagePrinter.PrintMessage(partNumber + " not found", ImportanceLevel.Mid);
                }

                //string data = "accountId=70600&searchText=" + HttpUtility.HtmlEncode(partNumber);
                ////var html = PageRetriever.WriteToServer("https://obx.hdsmith.com/OrderBaseXpressCS/Hhc/Search", data, true);

                //var jss = new JavaScriptSerializer();
                //var jso = jss.Deserialize<DataSerialization>(html);

                //if (jso.Pagination != null && jso.Pagination.TotalResults != 0)
                //{
                //    html = jso.ResultsHtml;
                //    var htmlDoc = CreateDoc(html);
                //    var tagProduct = htmlDoc.DocumentNode.SelectNodes("//ul[contains(@class,'result-tiles')]/li[contains(@class,'result-tile')]");
                //    if (tagProduct != null)
                //    {
                //        foreach (var itemProduct in tagProduct)
                //        {
                //            string itemPartnumber = itemProduct.SelectSingleNode(".//span[@class='product-item-number']").InnerTextOrNull();
                //            if (itemPartnumber != null)
                //                itemPartnumber = itemPartnumber.Replace("-", "").Replace("Item #:", "").Trim();
                //            if (itemPartnumber.ToLower() == partNumber.ToLower().Replace("-", "").Trim())
                //            {
                //                wi.Brand = itemProduct.SelectSingleNode(".//span[@class='product-vendor']").InnerTextOrNull();
                //            }
                //        }
                //    }
                //}

                pqi.Processed = true;
            }
            catch (Exception e)
            {
                MessagePrinter.PrintMessage(e.Message, ImportanceLevel.Critical);
            }
            OnItemLoaded(null);
            MessagePrinter.PrintMessage(partNumber + " processed");
            StartOrPushPropertiesThread();
        }

        private void ProcessProductInfo(ProcessQueueItem pqi)
        {
            if (cancel)
                return;
            ExtWareInfo wi = (ExtWareInfo)pqi.Item;

            try
            {
                var html = PageRetriever.ReadFromServer(pqi.URL, true);
                var htmlDoc = CreateDoc(html);

                wi.UPC = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='lblNDC']").InnerTextOrNull();
                wi.Brand = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='lbVendorName']").InnerTextOrNull();
                var tagImage = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='HDItemImage']");
                if (tagImage != null)
                {
                    wi.GeneralImage = tagImage.GetAttributeValue("src", null);
                    if(wi.GeneralImage != null && wi.GeneralImage.EndsWith("4-0251d90ced976b0f"))
                    {
                        wi.GeneralImage = "no image";
                    }
                    else { if (wi.GeneralImage.StartsWith("//")) 
                    {
                        wi.GeneralImage = "https:" + wi.GeneralImage;
                    } }
                }

                wi.Title = htmlDoc.DocumentNode.SelectSingleNode("//fieldset[@class='fullHeightWidth']/table[@class='width100']//tr[./td[1]/span[@class='HDDesc' and contains(text(),'Label Name')]]/td[2]").InnerTextOrNull();
                wi.Title2 = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='lblItemDesc']").InnerTextOrNull();
                if (!string.IsNullOrEmpty(wi.Title2))
                {
                    wi.Title2=wi.Title2.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                }
                wi.Description = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='lbHhcLongDescription']").InnerTextOrNull();
                if (string.IsNullOrEmpty(wi.Description))
                {
                    wi.Description = htmlDoc.DocumentNode.SelectSingleNode("//fieldset[@class='fullHeightWidth']/table[@class='width100']//tr[./td[1]/span[@class='HDDesc' and contains(text(),'Therapeutic Description')]]/td[2]").InnerTextOrNull();
                }

                string Price = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='lbPrice']").InnerTextOrNull();
                if(!string.IsNullOrEmpty(Price)){
                    wi.Cost = ParseDouble(Price.Replace("$","").Trim());
                }

                string form = htmlDoc.DocumentNode.SelectSingleNode("//fieldset[@class='fullHeightWidth']/table//tr[./td[1]/span[@class='HDDesc' and contains(text(),'Form')]]/td[2]").InnerTextOrNull();
                string packSize = htmlDoc.DocumentNode.SelectSingleNode("//fieldset[@class='fullHeightWidth']/table//tr[./td[1]/span[@class='HDDesc' and contains(text(),'Pack Size')]]/td[2]").InnerTextOrNull();
                string strength = htmlDoc.DocumentNode.SelectSingleNode("//fieldset[@class='fullHeightWidth']/table//tr[./td[1]/span[@class='HDDesc' and contains(text(),'Strength')]]/td[2]").InnerTextOrNull();

                if (!string.IsNullOrEmpty(form))
                {
                    wi.Specification += "Form~" + form + "~1^";
                }
                if (!string.IsNullOrEmpty(packSize))
                {
                    wi.Specification += "Pack Size~" + packSize + "~1^";
                }
                if (!string.IsNullOrEmpty(strength))
                {
                    wi.Specification += "Strength~" + strength + "~1^";
                }
                if (!string.IsNullOrEmpty(wi.Specification))
                {
                    wi.Specification = "Specification##" + wi.Specification.Trim('^');
                }
                 
                AddWareInfo(wi);
                pqi.Processed = true;
            }
            catch (Exception e)
            {
                MessagePrinter.PrintMessage(e.Message, ImportanceLevel.Critical);
            }
            OnItemLoaded(null);
            MessagePrinter.PrintMessage("Product info " + wi.PartNumber + " processed");
            StartOrPushPropertiesThread();
        }

        private List<ExtWareInfo> GetItemsNumberFromFile()
        {
            List<ExtWareInfo> Items = new List<ExtWareInfo>();

            var localPath = Settings.Location;
            var FielePath = localPath.Substring(0, localPath.Length - (localPath.Length - localPath.LastIndexOf('\\')) + 1) + "\\Data\\HDS_CAT.csv";

            using (var sr = File.OpenText(FielePath))
            {
                using (var csv = new CsvReader(sr, true, ','))
                {
                    var headers = csv.GetFieldHeaders();
                    while (csv.ReadNextRecord())
                    {
                        ExtWareInfo wi = new ExtWareInfo();
                        wi.PartNumber = csv["Item Number"].Trim('@');
                        wi.PartNumberMidified = correctPartNumber(wi.PartNumber);
                        wi.tmp = csv["tmp"];
                        //wi.Title = csv["Title"];
                        wi.listPrice1 = csv["list price1"];
                        wi.listPrice2 = csv["list price2"];
                        Items.Add(wi);
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

        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;
            if (item.ItemType == 1)
                act = ProcessBrandsListPage;
            else if (item.ItemType == 2)
                act = ProcessProductInfo;
            else act = null;

            return act;
        }


    }
}
