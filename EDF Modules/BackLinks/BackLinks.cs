using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using BackLinks;
using Databox.Libs.BackLinks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WheelsScraper
{
    public static class StringExt {
        public static string ClearQ(this string str){
            return str.Replace("'","").Replace("N/A","");
        }
        public static string GetStrQ(string str) {
            return str != null ? str.ClearQ() : string.Empty;
        }
    }
    public class BackLinks : BaseScraper
    {
        public BackLinks()
        {
            Name = "BackLinks";
            Url = "http://www.BackLinks.com/";
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
       
            MessagePrinter.PrintMessage("Starting login");
            var login = GetLoginInfo();
            if (login == null)
                throw new Exception("No valid login found");

            string loginURL = Url + "login.php?FormAction=login&FormName=Login&isAjax=1&Login=" + login.Login + "&Password=" + login.Password;
            string data = "Login=" + login.Login + "&Password=" + login.Password;
          //  PageRetriever.Referer = Url;
            //PageRetriever.GetRandomProxy();
            //MessagePrinter.PrintMessage("UseProxy:" + PageRetriever.proxy.Address.OriginalString);
            var html = PageRetriever.ReadFromServer(loginURL, true, true);

           // html = PageRetriever.ReadFromServer(Url + "advertisers/myaccount.php", true);

            if (html.Contains("Authentication Success"))
            {
                MessagePrinter.PrintMessage("Login done.");
                return true;
            }
            return false;
        }

        protected override void RealStartProcess()
        {
            string ecomerceLink = "t=block&l=http://www.ecommercespot.com";
            var html = PageRetriever.ReadFromServer(Url + "advertisers/dir.php?" + ecomerceLink, true);

       

            // find categories 
            var doc = CreateDoc(html);
            var _select = doc.DocumentNode.SelectNodes("//option[ancestor::select[@name='CID']]");

            foreach (var option in _select)
            {
                var catId = option.AttributeOrNull("value");
                var catName = option.NextSibling.InnerTextOrNull();
                if (extSett.CategotySearchList.Any(n => n.CategoryName!= null && n.CategoryName.Contains(catName)))
                {
                    MessagePrinter.PrintMessage("Get category: " + catName);
                    if (!extSett.CategotyFoundList.Any(n=>n.Contains(catName)))
                        extSett.CategotyFoundList.Add(catName);

                    var catUrl = Url + "advertisers/links.php?" + ecomerceLink + "&CID=" + catId + "&f=all&cr=US&lng=en";
                    lstProcessQueue.Add(new ProcessQueueItem { URL = catUrl, ItemType = 2, Name = catName, Item ="1" });
                }
            }

           
            StartOrPushPropertiesThread();
        }

        protected void ProcessCategoryListPage(ProcessQueueItem pqi)
        {
            var page = (string)pqi.Item;
            if (cancel)
                return;

            MessagePrinter.PrintMessage("Start process category " + pqi.Name + ", page" + page);

            var html = PageRetriever.ReadFromServer(pqi.URL, true);
            var doc = CreateDoc(html);
            
            // find links on page
            var links = doc.DocumentNode.SelectNodes("//table//td[contains(@class,'DataTD') and contains(@class ,'smallb')]");
            if (links == null)
            {
                pqi.Processed = true;
                return;
            }

            foreach (var link in links) {
                
                var wi = new ExtWareInfo();
                wi.Title = link.SelectSingleNode(".//span[@class='smallb']").InnerTextOrNull();
                MessagePrinter.PrintMessage("  Process " + pqi.Name + " / "+wi.Title);
                var landingPage = link.SelectSingleNode(".//span[@class='smallb']/following-sibling::text()").InnerTextOrNull().Replace(Environment.NewLine, "").Trim();
                if (landingPage.Contains(")"))
                    wi.LandingPage = landingPage.Substring(0, landingPage.IndexOf(")")+1); 
                wi.PriceText = link.SelectSingleNode(".//font[@class='smallb']").InnerTextOrNull();
                Regex regPrice = new Regex(@"[0-9]+(\.[0-9]+)?");
                var price = !string.IsNullOrEmpty(wi.PriceText) ? regPrice.Match(wi.PriceText).Value : "0";
                wi.Price = decimal.Parse(price, CultureInfo.InvariantCulture);

                //            0        1        2        3          4           5        6      7
                //showUrl(page_id, GooglePR, MozRank, AlexaRank, BackLinks, DomainAge, MOZDA, LinkUrl)
                //showUrl(917164, '23.62', '4.9',  '10130615', '792', '12', '23.62', '')
                var showUrl = link.SelectSingleNode(".//span[@class='smallb']").AttributeOrNull("onclick");
                Regex regVal = new Regex(@"\(.*?\)");
                showUrl = regVal.Match(showUrl).Value.Replace("(","").Replace(")","");
                var arr = showUrl.Split(',');
                if (arr.Length > 0) {
                    wi.id = StringExt.GetStrQ(arr[0]);
                    wi.Moz = StringExt.GetStrQ(arr[6]);
                    wi.Backlinks = StringExt.GetStrQ(arr[4]);
                    wi.DomainAge = StringExt.GetStrQ(arr[5]);
                    wi.MozRank = StringExt.GetStrQ(arr[2]);
                    wi.Alexa = StringExt.GetStrQ(arr[3]);
                }
                // get url
                if (!string.IsNullOrEmpty(wi.id)) {
                    var retriveUrl = Url + "advertisers/rpc/getlink.php";
                    string data = "id=" + wi.id;
                    var webSite = PageRetriever.WriteToServer(retriveUrl, data, true, true);
                    var strIndex = "nullrefer.com/?";
                    webSite = webSite.Substring(webSite.IndexOf(strIndex) + strIndex.Length);
                    wi.WebSiteAdress = webSite;
                }

                AddWareInfo(wi);
                OnItemLoaded(wi);

            }
            // has pagination?
            var next = doc.DocumentNode.SelectSingleNode(".//a[text()='Next']");
            if (next != null) {
                var next_url = Url + "advertisers/" + next.AttributeOrNull("href");
          
                var pageNum =  next_url[next_url.Length - 1] ;
                lstProcessQueue.Add(new ProcessQueueItem { URL = next_url, ItemType = 2, Name = pqi.Name, Item = pageNum.ToString() });
            }

            pqi.Processed = true;
            MessagePrinter.PrintMessage("End process category "+ pqi.Name +", page" + page);
            StartOrPushPropertiesThread();
        }

        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;
            if (item.ItemType == 2)
                act = ProcessCategoryListPage;
            else act = null;

            return act;
        }
    }
}
