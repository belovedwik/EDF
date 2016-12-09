using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using PeoplePerHour;
using Databox.Libs.PeoplePerHour;

namespace WheelsScraper
{
    public class PeoplePerHour : BaseScraper
    {
        readonly string EndSearchText = @"-hourlies?ref=search&filter=all";
        public PeoplePerHour()
        {
            Name = "PeoplePerHour";
            Url = "https://www.peopleperhour.com/";
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
            lstProcessQueue.Add(new ProcessQueueItem { URL = Url, ItemType = 1 });
            StartOrPushPropertiesThread();
        }

        protected void ProcessSearchPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return;

            if (extSett.TagList.Count() == 0)
            {
                MessagePrinter.PrintMessage("TAGs is empty, exit", ImportanceLevel.High);
                return;
            }

            foreach (var tag in extSett.TagList) {
                lock (this)
                    lstProcessQueue.Add(new ProcessQueueItem { ItemType = 2, Name = tag.SearchTag, URL = (Url + tag.SearchTag.Trim() + EndSearchText), Item = tag.SearchKeyword });
            }
            
            pqi.Processed = true;
            OnItemLoaded(null);

            MessagePrinter.PrintMessage("Main page processed");
            StartOrPushPropertiesThread();
        }

        private void ProcessSearchResultPage(ProcessQueueItem pqi) {

            var keywords = (string)pqi.Item ?? "";
            var searchTag = pqi.Name;

            var html = PageRetriever.ReadFromServer(pqi.URL);
            var doc = CreateDoc(html);

            var main_profiles = doc.DocumentNode.SelectNodes("//div[contains(@class, 'hourlie-wrapper')]");

            if (main_profiles == null)
            {
                MessagePrinter.PrintMessage("Not found any results with text: " + searchTag, ImportanceLevel.High);
                return;
            }
            foreach (var profile in main_profiles)
            {

                var wi = new ExtWareInfo();

                var price = profile.SelectSingleNode(".//div[contains(@class,'price-container')]/span").InnerTextOrNull();

                wi.Title = profile.SelectSingleNode(".//h5[@class='hourlie__title']/a").InnerTextOrNull();
                wi.Country = profile.SelectSingleNode(".//span[contains(@class,'user-country')]").InnerTextOrNull();
                wi.ProfileLink = profile.SelectSingleNode(".//div[contains(@class,'user-info-container')]/a").AttributeOrNull("href");
                wi.Link = profile.SelectSingleNode(".//h5[@class='hourlie__title']/a").AttributeOrNull("href");
                wi.Delivery = profile.SelectSingleNode(".//span[@class='hourlie-info-value']").InnerTextOrNull();
                wi.Name = profile.SelectSingleNode(".//div[contains(@class,'user-info-container')]/a").InnerTextOrNull();
                wi.Price = decimal.Parse(price != null ? price : "0");
                wi.TAGs = "";

                var keywordList = keywords.Split(' ');
                if (keywordList.Count() > 0) {
                    foreach (var word in keywordList) {
                        if (wi.Title.Contains(word)) {
                            wi.TAGs = searchTag;
                            break;
                        }
                    }
                }    

               

                lock (this)
                    lstProcessQueue.Add(new ProcessQueueItem { ItemType = 10, Item = wi });
            }

            // has pagination?
            var next = doc.DocumentNode.SelectSingleNode(".//ul[@id='hourlies-listing-pager']/li/a[@class='next']");
            if (next != null)
            {
                var next_url = next.AttributeOrNull("href");
                if (!next_url.Contains(Url))
                    next_url = Url + next_url;

                ProcessQueueItem link_next = new ProcessQueueItem { URL = next_url, ItemType = 2, Item = keywords, Name = searchTag };
                lock (this)
                    lstProcessQueue.Add(link_next);
            }
            pqi.Processed = true;

        
        }
        private void ProcessProductPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return;
            var wi = (ExtWareInfo)pqi.Item;

            MessagePrinter.PrintMessage("Get project info:" + wi.Title);

            var html = PageRetriever.ReadFromServer(wi.Link);
            var doc = CreateDoc(html);
            var userProfile = doc.DocumentNode;



            var _info = userProfile.SelectNodes("//div[contains(@class,'detailed-information')]/ul/li");
            var _sales = "0";
            var _view = "0";
           
            if (_info != null)
                foreach (var info_node in _info)
                {
                    if (info_node.ChildNodes != null){
                        if (info_node.ChildNodes[1].InnerText.Contains("Sales"))
                            _sales = info_node.ChildNodes[3].InnerText;
                        if (info_node.ChildNodes[1].InnerText.Contains("Views"))
                            _view = info_node.ChildNodes[3].InnerText;
                    } 
                }
            wi.Sales = Int32.Parse(_sales.Replace(",", "").Replace(" ", ""));
            wi.Views = Int32.Parse(_view.Replace(",", "").Replace(" ", ""));
            var _favorites = userProfile.SelectSingleNode("//span[contains(@class,'count-stars')]").InnerTextOrNull();
            if (_favorites != null)
                wi.Favorites = Int32.Parse(_favorites);
            wi.ReviewRating = userProfile.SelectSingleNode("//div[@data-content-selector='.popover-feedback']/div/span").InnerTextOrNull();
            wi.Responce = userProfile.SelectSingleNode("//div[@data-content-selector='.popover-response']/div/span").InnerTextOrNull();
            wi.Review = userProfile.SelectSingleNode("//div[contains(@class,'feedbacks-list-container')]/h2").InnerTextOrNull();
            

            AddWareInfo(wi);
            OnItemLoaded(wi);
            
            pqi.Processed = true;
            StartOrPushPropertiesThread();

        }

        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;
            if (item.ItemType == 1)
                act = ProcessSearchPage;
            else if (item.ItemType == 2)
                act = ProcessSearchResultPage;
            else if (item.ItemType == 10)
                act = ProcessProductPage;
            else act = null;

            return act;
        }
    }
}
