using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using Eswimelite;
using Databox.Libs.Eswimelite;
using System.Globalization;

namespace WheelsScraper
{
    public class Eswimelite : BaseScraper
    {
        public Eswimelite()
        {
            Name = "Eswimelite";
            Url = "http://qswimwear.com/";
            PageRetriever.Referer = Url;
            WareInfoList = new List<ExtWareInfo>();
            Wares.Clear();
            //BrandItemType = 2;

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

        protected void ProcessCategoriesList(ProcessQueueItem pqi)
        {
            if (cancel)
                return;

            MessagePrinter.PrintMessage("Get Categories List .. ");
            
            var html = PageRetriever.ReadFromServer(pqi.URL);
            var doc = CreateDoc(html);

            var main_categories = doc.DocumentNode.SelectNodes("//ul[@id='main-nav']/li//a");
            if (main_categories == null)
                return;


            foreach (var cats in main_categories)
            {
                var name = cats.InnerTextOrNull();
                if (string.IsNullOrEmpty(name))
                {
                    MessagePrinter.PrintMessage("Empty Category name", ImportanceLevel.High);
                    continue;
                }

                var url = cats.AttributeOrNull("href");
                var wi = new ExtWareInfo { Name = name, URL = url };

                lock (this)
                    lstProcessQueue.Add(new ProcessQueueItem { ItemType = 2, Item = wi, URL = url });
            }
            pqi.Processed = true;
            OnItemLoaded(null);

            MessagePrinter.PrintMessage("CategoriesList list processed .. ok");
            StartOrPushPropertiesThread();
        }

        private string CheckUrl( string url){
         
            Uri baseUri = new Uri(Url);

            if (!url.Contains(Url))
                baseUri = new Uri(baseUri, url);
            else
                baseUri = new Uri(url);

            return baseUri.AbsoluteUri;
        }

        private void ProcessProductList(ProcessQueueItem pqi)
        {
            if (cancel)
                return;

            pqi.URL = CheckUrl(pqi.URL);

            var html = PageRetriever.ReadFromServer(pqi.URL);
            var doc = CreateDoc(html);

            var productList = doc.DocumentNode.SelectNodes("//div[@id='product-loop']/div[contains(@class, 'product-index')]");
            if (productList == null)
            { // category has no produsts .... => it's subcategory
                MessagePrinter.PrintMessage("Empty SubCategory .. " + pqi.Name);
                //TODO: Print it before you send request, usual it follows after check cancellation
                return;
            }

            MessagePrinter.PrintMessage("Get Product List .. " + pqi.Name);

            foreach (var prod in productList)
            {
                var prod_Link = prod.SelectSingleNode(".//div[@class='prod-image']/a");
                if (prod_Link == null)
                { // product has no link
                    MessagePrinter.PrintMessage("Product has no link .. ", ImportanceLevel.High);
                    continue;
                }

                var name = prod_Link.AttributeOrNull("title");
                var url = prod_Link.AttributeOrNull("href");

                var wi = new ExtWareInfo { Name = name, URL = url };

                lock (this)
                    lstProcessQueue.Add(new ProcessQueueItem { ItemType = 10, Item = wi, URL = url, Name = name });
            }

            var next = doc.DocumentNode.SelectSingleNode("//div[@id='pagination']/span[@class='current']");
            if (next != null && next.NextSibling.NextSibling != null) // has pagination
            {
                var name = next.NextSibling.NextSibling.InnerTextOrNull();
                var url = next.NextSibling.NextSibling.AttributeOrNull("href");

                ProcessQueueItem link_next = new ProcessQueueItem { URL = url, Name = name, ItemType = 2 };
                lock (this)
                    lstProcessQueue.Add(link_next);
            }

            //OnItemLoaded(null);

            pqi.Processed = true;
            MessagePrinter.PrintMessage("Product List end .. " + pqi.Name);
            StartOrPushPropertiesThread();

        }

        private void ProcessProductPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return;

            pqi.URL = CheckUrl(pqi.URL);

            var wi = (ExtWareInfo)pqi.Item;
            MessagePrinter.PrintMessage("Get product info:" + wi.Name);
            var html = PageRetriever.ReadFromServer(pqi.URL);

            var doc = CreateDoc(html);

            wi.Action = "ADD";
            wi.URL = pqi.URL;

            wi.ImageUrl = doc.DocumentNode.SelectSingleNode("//div[@id='product-photos']/div[contains(@class, 'bigimage')]/img").AttributeOrNull("src");
            wi.ImageUrl = wi.ImageUrl.Substring(0, wi.ImageUrl.LastIndexOf("?"));

            var ImgList = doc.DocumentNode.SelectNodes("//div[@id='product-photos']//div[contains(@class, 'slide')]/a");
            wi.ImagesList = string.Join(",", ImgList.Select(i => i.AttributeOrNull("data-image").Substring(0, i.AttributeOrNull("data-image").LastIndexOf("?") )  ).ToList() );

            wi.Name = wi.ProductTitle = doc.DocumentNode.SelectSingleNode("//div[@id='product-description']/h1").InnerTextOrNull();

            var MetaDesc = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
            wi.MetaDescription = (!string.IsNullOrEmpty(MetaDesc.AttributeOrNull("content"))) ? MetaDesc.AttributeOrNull("content") : wi.ProductTitle;
            var KeyWords = doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
            wi.MetaKeywords = (!string.IsNullOrEmpty(KeyWords.AttributeOrNull("content"))) ? KeyWords.AttributeOrNull("content") : wi.ProductTitle;

            var price = doc.DocumentNode.SelectSingleNode("//p[@id='product-price']/span").InnerTextOrNull();
            wi.Cost = wi.MSRP = wi.WebPrice = double.Parse(price.Replace("$", ""), new CultureInfo("en"));

            wi.Description = doc.DocumentNode.SelectSingleNode("//div[@id='product-description']//div[contains(@itemprop, 'description')]").InnerTextOrNull();

            var BulletPoints = doc.DocumentNode.SelectNodes("//div[contains(@itemprop, 'description')]//ul/li");
            if (BulletPoints != null) {
                wi.BulletPoint = String.Join("~!~", BulletPoints.Select(b => b.InnerTextOrNull()).ToList());
            }

            var Breadcrumb = doc.DocumentNode.SelectSingleNode("//div[@id='breadcrumb']").InnerTextOrNull().Split('/');

            wi.MainCategory = Breadcrumb[1].Trim() ?? "";
            wi.SubCategory = Breadcrumb[2].Trim() ?? "";
            wi.Section = "";
            wi.PartNumber = doc.DocumentNode.SelectSingleNode("//form/div[@class='select']//select/option").AttributeOrNull("data-sku"); 

            var options = doc.DocumentNode.SelectSingleNode("//form/div[@class='swatch clearfix']");
            if (options != null)
            {
                wi.ProductType = 2; // 1 - universal, 2- with options
                wi.PrimaryOptionTitle = options.SelectSingleNode(".//h5").InnerTextOrNull();
                
                foreach (var option in options.SelectNodes(".//label")) {
                    wi.PrimaryOptionChoice = option.InnerTextOrNull();
                    wi.Specification = "Item Spec##" + wi.PrimaryOptionTitle + "-" + wi.PrimaryOptionChoice;
                    
                    var wi2 = (ExtWareInfo)wi.Clone();
                    wi2.PartNumber += "-" + wi.PrimaryOptionChoice.Replace(" ","");
                    AddWareInfo(wi2);
                    OnItemLoaded(wi2);
                }

            } 
            else {
                wi.ProductType = 1; // 1 - universal, 2- with options
                AddWareInfo(wi);
                OnItemLoaded(wi);
            }

            wi.Processed = pqi.Processed = true;
           

            StartOrPushPropertiesThread();
        }


        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;
            if (item.ItemType == 1)
                act = ProcessCategoriesList;
            else if (item.ItemType == 2)
                act = ProcessProductList;
            else if (item.ItemType == 10)
                act = ProcessProductPage;
            else act = null;

            return act;
        }
    }
}
