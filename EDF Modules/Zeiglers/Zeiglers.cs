using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using Zeiglers;
using Databox.Libs.Zeiglers;
using System.Runtime.Serialization;
using Newtonsoft.Json;


namespace WheelsScraper
{
    public class Zeiglers : BaseScraper
    {
        public Zeiglers()
        {
            Name = "Zeiglers";
            Url = "http://www.zieglers.com";
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
            lstProcessQueue.Add(new ProcessQueueItem { URL = Url, ItemType = 1});
            StartOrPushPropertiesThread();
        }

	

        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;
            if (item.ItemType == 1)
                act = ProcessCategoryList;
			else if (item.ItemType == 2)
                act = ProcessProductList;
			else if (item.ItemType == 10)
				act = ProcessProductPage;
            else act = null;

            return act;
        }

		protected void ProcessCategoryList(ProcessQueueItem pqi)
		{
			if (cancel)
				return;

			var html = PageRetriever.ReadFromServer(pqi.URL);
			var doc = CreateDoc(html);

			pqi.Processed = true;
			var main_categories = doc.DocumentNode.SelectSingleNode("//ul[@class='category-list']");
			if (main_categories == null)
				return;
			var categories = main_categories.SelectNodes(".//a");
			if (categories == null)
				return;

			MessagePrinter.PrintMessage("Get Categories List");
			
			foreach (var cats in categories)
			{
				var name = cats.InnerTextOrNull();
				if (string.IsNullOrEmpty(name))
					continue;
				var url = cats.AttributeOrNull("href");
				var wi = new ExtWareInfo { Name = name, URL = url };

				//AddWareInfo(wi);

				lock (this)
					lstProcessQueue.Add(new ProcessQueueItem { ItemType = 2, Item = wi, URL = url });
			}

			OnItemLoaded(null);

			pqi.Processed = true;
			MessagePrinter.PrintMessage("Categories List .. ok");
			StartOrPushPropertiesThread();
		}

		private void ProcessProductList(ProcessQueueItem pqi)
		{
			if (cancel)
				return;
			Uri baseUri = new Uri(Url);

			if (!pqi.URL.Contains(Url))
				baseUri = new Uri(baseUri, pqi.URL);
			else
				baseUri = new Uri(pqi.URL);


			var html = PageRetriever.ReadFromServer(baseUri.AbsoluteUri);
			var doc = CreateDoc(html);

			pqi.Processed = true;

			var productList = doc.DocumentNode.SelectNodes("//ul[@class='ProductList ']/li");
			if (productList == null) // category has no produsts .... => it's subcategory
				return;

			MessagePrinter.PrintMessage("Get Product List .. " + pqi.Name);

			foreach (var prod in productList)
			{
				var prod_Link = prod.SelectSingleNode(".//div[@class='ProductDetails']/strong/a");
				if (prod_Link == null) // product has no link
					continue;

				var name = prod_Link.InnerTextOrNull();
				var url = prod_Link.AttributeOrNull("href");

				var wi = new ExtWareInfo { Name = name, URL = url };

				lock (this)
					lstProcessQueue.Add(new ProcessQueueItem { ItemType = 10, Item = wi, URL = url, Name = name });
			}

			var next = doc.DocumentNode.SelectSingleNode("//div[@class='CategoryPagination']/div[@class='FloatRight']/a");
			if (next != null) // has pagination
			{
				var name = next.InnerTextOrNull();
				var url = next.AttributeOrNull("href");

				ProcessQueueItem link_next = new ProcessQueueItem { URL = url, Name = name };

				lock(this)
					lstProcessQueue.Add(new ProcessQueueItem { ItemType = 2, Item = link_next, URL = url });

			}

			//OnItemLoaded(null);

			pqi.Processed = true;
			MessagePrinter.PrintMessage("Product List end .. " + pqi.Name);
			StartOrPushPropertiesThread();

			
		}

		

        private void ProcessProductPage(ProcessQueueItem pqi)
        {
            var wi = (ExtWareInfo)pqi.Item;
            MessagePrinter.PrintMessage("Get product info:" + wi.Name);
            var html = PageRetriever.ReadFromServer(pqi.URL);

            var doc = CreateDoc(html);
            pqi.Processed = true;

			var prodDetail = doc.DocumentNode.SelectSingleNode("//div[@id='ProductDetails']");
			
			wi.ProductTitle = prodDetail.SelectSingleNode(".//div[@class='BlockContent']/h1").InnerTextOrNull();
			wi.ProductDescription = prodDetail.SelectSingleNode(".//div[@class='ProductDescriptionContainer']").InnerTextOrNull();
			wi.ImageUrl = prodDetail.SelectSingleNode(".//div[@class='ProductThumbImage']/a").AttributeOrNull("href");

			var imgNodes = prodDetail.SelectNodes(".//div[@class='ProductTinyImageList']/ul/li");
			if (imgNodes != null)
			{
				List<string> imgList = new List<string>();
				foreach (var img in imgNodes)
				{
					var imgRel = img.SelectSingleNode(".//div[@class='TinyOuterDiv']/div/a").AttributeOrNull("rel");
					dynamic jsonImg = JsonConvert.DeserializeObject(imgRel);
					if (jsonImg.largeimage != null)
						imgList.Add( (string)jsonImg.largeimage );
				}
				wi.ImagesList = "\"" + string.Join(",", imgList) + "\"";
			}

			wi.Price_string = prodDetail.SelectSingleNode(".//em[contains(@class, 'VariationProductPrice')]").InnerTextOrNull();

			/*
			var options = prodDetail.SelectNodes(".//div[@class='DetailRow']");
			foreach (var option in options) {
				if (option.SelectSingleNode(".//div[class='Label']").InnerTextOrNull().Contains("Weight")) { 
					wi.Weight = option.SelectSingleNode(".//div[class='Value']").InnerTextOrNull()
					break;
				}
			}
			*/
			wi.Weight = prodDetail.SelectSingleNode(".//span[contains(@class, 'VariationProductWeight')]").InnerTextOrNull();

			AddWareInfo(wi);
			OnItemLoaded(wi);
        }
    }
}
