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
using System.Globalization;
using System.Net;
using System.Text;
using System.IO;


namespace WheelsScraper
{
    public class Zeiglers : BaseScraper
    {
        public Zeiglers()
        {
            Name = "Zeiglers";
            Url = "http://www.zieglers.com/";
            PageRetriever.Referer = Url;
            WareInfoList = new List<ExtWareInfo>();
            Wares.Clear();
            //TODO: If BrandItemType = ItemType of current pqi procces it save export file
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
                if (string.IsNullOrEmpty(name)) {
                    MessagePrinter.PrintMessage("Empty Category name", ImportanceLevel.High);
                    continue;
                }
				
				var url = cats.AttributeOrNull("href");
				var wi = new ExtWareInfo { Name = name, URL = url };

				lock (this)
					lstProcessQueue.Add(new ProcessQueueItem { ItemType = 2, Item = wi, URL = url });
			}
            pqi.Processed = true;
            //AddWareInfo(wi);
			OnItemLoaded(null);

			
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

			var productList = doc.DocumentNode.SelectNodes("//ul[@class='ProductList ']/li");
            if (productList == null)
            { // category has no produsts .... => it's subcategory
                MessagePrinter.PrintMessage("Empty SubCategory .. " + pqi.Name); 
                //TODO: Print it before you send request, usual it follows after check cancellation
                return;
            } 
			
			MessagePrinter.PrintMessage("Get Product List .. " + pqi.Name);

			foreach (var prod in productList)
			{
				var prod_Link = prod.SelectSingleNode(".//div[@class='ProductDetails']/strong/a");
                if (prod_Link == null)
                { // product has no link
                    MessagePrinter.PrintMessage("Product has no link .. ", ImportanceLevel.High);
                    continue;
                }
					
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

				ProcessQueueItem link_next = new ProcessQueueItem { URL = url, Name = name, ItemType = 2 };
                lock(this)
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

            var wi = (ExtWareInfo)pqi.Item;
            MessagePrinter.PrintMessage("Get product info:" + pqi.Name);
            var html = PageRetriever.ReadFromServer(pqi.URL);

            var doc = CreateDoc(html);


            wi.ProdID = doc.DocumentNode.SelectSingleNode("//input[@name='product_id']").AttributeOrNull("value");
            wi.URL = pqi.URL;

            var prodDetail = doc.DocumentNode.SelectSingleNode("//div[@id='ProductDetails']");
			
            wi.Action = "ADD";
      
			wi.ProductTitle = prodDetail.SelectSingleNode(".//div[@class='BlockContent']/h1").InnerTextOrNull();


			wi.ProductDescription = prodDetail.SelectSingleNode(".//div[@class='ProductDescriptionContainer']").InnerTextOrNull();

            if (prodDetail.SelectSingleNode(".//div[@class='ProductDescriptionContainer']//ul/li") != null)
            { 
                //product has details
                var bullets = prodDetail.SelectNodes(".//div[@class='ProductDescriptionContainer']//ul/li");
                List<string> bulletList = new List<string>();
                
                foreach (var bullet in bullets)
                    bulletList.Add(bullet.InnerTextOrNull());

                wi.BulletPoint = string.Join("~!~", bulletList);
            }
            wi.ImageUrl = prodDetail.SelectSingleNode(".//div[@class='ProductThumbImage']/a").AttributeOrNull("href");
           
            var MetaDesc = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
            wi.MetaDescription = (!string.IsNullOrEmpty(MetaDesc.AttributeOrNull("content"))) ? MetaDesc.AttributeOrNull("content") : wi.ProductTitle;

            var KeyWords = doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
            wi.MetaKeywords = (!string.IsNullOrEmpty(KeyWords.AttributeOrNull("content"))) ? KeyWords.AttributeOrNull("content") : wi.ProductTitle;

            var Breadcrumb = prodDetail.SelectSingleNode(".//div[@id='ProductBreadcrumb']");

            wi.MainCategory = Breadcrumb.SelectSingleNode(".//ul/li[2]").InnerTextOrNull();
            wi.SubCategory = Breadcrumb.SelectSingleNode(".//ul/li[3]").InnerTextOrNull();
            wi.Section = Breadcrumb.SelectSingleNode(".//ul/li[4]").InnerTextOrNull();

            var Price_string = prodDetail.SelectSingleNode(".//em[contains(@class, 'VariationProductPrice')]").InnerTextOrNull();
            int start_index = Price_string.IndexOf(" ");
            Price_string = Price_string.Substring(0, start_index > 0 ? start_index : Price_string.Length);

            wi.WebPrice = wi.Cost = Double.Parse(Price_string.Replace("$",""), new CultureInfo("en"));
        
			var imgNodes = prodDetail.SelectNodes(".//div[@class='ProductTinyImageList']/ul/li");
            if (imgNodes != null)
            {
                List<string> imgList = new List<string>();
                foreach (var img in imgNodes)
                {
                    var imgRel = img.SelectSingleNode(".//div[@class='TinyOuterDiv']/div/a").AttributeOrNull("rel");
                    dynamic jsonImg = JsonConvert.DeserializeObject(imgRel);
                    if (jsonImg.largeimage != null)
                        imgList.Add((string)jsonImg.largeimage);
                }
                //wi.ImagesList = "\"" + string.Join(",", imgList) + "\"";
                wi.ImagesList = string.Join(",", imgList);
            }
            else {
                MessagePrinter.PrintMessage("Product has no images " + wi.Name, ImportanceLevel.High);
            }

			var options = prodDetail.SelectNodes(".//div[@class='DetailRow']");
			foreach (var option in options) {
                var option_text = option.SelectSingleNode(".//div[@class='Label']").InnerTextOrNull();
				/*if (option_text.Contains("Weight")) { 
					wi.Weight = option.SelectSingleNode(".//div[class='Value']").InnerTextOrNull();
				}*/
                if (option_text.Contains("Brand"))
                {
                    wi.Brand = option.SelectSingleNode(".//div[@class='Value']").InnerTextOrNull();
                    break;
				}
			}
			
            var weight = prodDetail.SelectSingleNode(".//span[contains(@class, 'VariationProductWeight')]").InnerTextOrNull();
            weight = weight.Substring(0, weight.IndexOf(" LBS"));
            wi.Weight = double.Parse(weight, new CultureInfo("en"));
            if (wi.Weight == 0)
                wi.Weight = 1;
            wi.Height = wi.Width = wi.Length = 
                wi.ShippingHeight = wi.ShippingWidth = wi.ShippingLength = 1;
            wi.ShippingWeight = wi.Weight;


            var SKU = prodDetail.SelectSingleNode(".//span[@class='VariationProductSKU']").InnerTextOrNull();
            wi.PartNumber = wi.Jobber = SKU; // ? = mspr;

            var productAttributeList = prodDetail.SelectSingleNode(".//div[@class='productAttributeList']");
            if (productAttributeList != null && productAttributeList.ChildNodes.Count > 0)
            {
                wi.ProductType = 2; // 1 - universal, 2- with options
                wi.PrimaryOptionTitle = productAttributeList.SelectSingleNode(".//div[@class='productAttributeLabel'][1]/label/span[@class='name']").InnerTextOrNull();
                wi.SecondaryOptionTitle = productAttributeList.SelectSingleNode(".//div[@class='productAttributeLabel'][2]/label/span[@class='name']").InnerTextOrNull();
                wi.ScondaryOptionChoice = productAttributeList.SelectSingleNode(".//div[@class='productAttributeValue'][2]/div[@class='productOptionViewSelect']/select/option[1]").InnerTextOrNull();

                var primaryOptionVal_Select = productAttributeList.SelectSingleNode(".//div[@class='productAttributeValue'][1]/div[@class='productOptionViewSelect']/select");
                 
                if (primaryOptionVal_Select != null)
                {
                    var primaryOptionVal_options = primaryOptionVal_Select.SelectNodes(".//option");
                    var selectName = primaryOptionVal_Select.AttributeOrNull("name");
                    // select
                    foreach (var option in primaryOptionVal_options)
                    {
                        wi.PrimaryOptionChoice = option.NextSibling.InnerTextOrNull();
                        if (!String.IsNullOrEmpty( option.AttributeOrNull("value") )) // first option is null
                        {
                            SendAdditionalRequest(selectName + "=" + option.AttributeOrNull("value"), ref wi);
                            AddWareInfoExt(wi);
                        } 
                    }
                }
                var primaryOptionVal_Radio = productAttributeList.SelectNodes(".//div[@class='productAttributeValue']/div[@class='productOptionViewRadio']/ul/li");
                if (primaryOptionVal_Radio != null)
                {   // radio
                    foreach (var option in primaryOptionVal_Radio)
                    {
                        wi.PrimaryOptionChoice = option.SelectSingleNode(".//label/span[@class='name']").InnerTextOrNull();
                        if (wi.PrimaryOptionChoice != null) // first option is null
                        {
                            var opt = option.SelectSingleNode(".//label/input");

                            SendAdditionalRequest(opt.AttributeOrNull("name") +"=" + opt.AttributeOrNull("value"), ref wi);
                            AddWareInfoExt(wi);
                        }
                            
                    }
                }             
            }
            else {
                wi.ProductType = 1; // 1 - universal, 2- with options
                AddWareInfoExt(wi);
            }

            pqi.Processed = true;
            
            StartOrPushPropertiesThread();
           // 
        }

        private void AddWareInfoExt(ExtWareInfo wi)
        {
            if (wi.ProductTitle.Contains(wi.PartNumber)) {
                wi.ProductTitle = wi.ProductTitle.Replace(wi.PartNumber, "").Trim();
            }
            wi.ImageUrl = wi.ImageUrl.Substring(0, wi.ImageUrl.LastIndexOf('?'));
            wi.Name = wi.Name;
            wi.Processed = true;

            ExtWareInfo wi2 = (ExtWareInfo)wi.Clone();

            AddWareInfo( wi2 );
            OnItemLoaded( wi2 );
        }

        public void SendAdditionalRequest(string attribute, ref ExtWareInfo wi) {

            if (cancel)
                return;

            Uri baseUri = new Uri("http://www.zieglers.com/remote.php");
           //baseUri = new Uri(baseUri, "http://www.zieglers.com/remote.php");

            string postData = "action=add&product_id=" + wi.ProdID + "&" + attribute + "&qty[]=1&w=getProductAttributeDetails";

            var request =  (HttpWebRequest)WebRequest.Create(baseUri.AbsoluteUri);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (!string.IsNullOrEmpty(responseString)) { 
                dynamic jsonVal = JsonConvert.DeserializeObject(responseString);
               
                wi.PartNumber = wi.Jobber =  (string)jsonVal.details.sku;
                wi.MSRP = wi.WebPrice = wi.Cost = Double.Parse((string)jsonVal.details.unformattedPrice, new CultureInfo("en"));
                wi.ImageUrl = (string)jsonVal.details.baseImage;
            }
               
         

        }
    }
}
