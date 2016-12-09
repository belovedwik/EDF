using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using BackLinks;
using Databox.Libs.BackLinks;

namespace WheelsScraper
{
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
            var html = PageRetriever.ReadFromServer(Url + "advertisers/dir.php?t=block&l=http://www.ecommercespot.com", true);

            // find categories 
            var doc = CreateDoc(html);
            var _categories = doc.DocumentNode.SelectNodes("//div[@id='admincontentbox']/div/table//td[@class='DataTD']//a");

            foreach (var cat in _categories) {
                var catName = cat.InnerTextOrNull();
                if (extSett.CategotyList.Any( n=> n.CategoryName.Contains(catName))) { 
                    lstProcessQueue.Add(new ProcessQueueItem { URL = cat.AttributeOrNull("href"), ItemType = 1, Name = catName });
                }
            }

           
            StartOrPushPropertiesThread();
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
