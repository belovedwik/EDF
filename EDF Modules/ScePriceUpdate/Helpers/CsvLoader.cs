#region using

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Databox.Libs.ScePriceUpdate;
using Ionic.Zip;
using log4net;
using LumenWorks.Framework.IO.Csv;
using ScePriceUpdate.Extensions;
using ScePriceUpdate.SCEApi;
using WheelsScraper;

#endregion

namespace ScePriceUpdate.Helpers
{
    public class CsvLoader
    {
        private readonly ScraperSettings _settings;
        private readonly ILog _log;

        public CsvLoader(ILog log, ScraperSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        private sceApi GetApiClient()
        {
            if (string.IsNullOrEmpty(_settings.SCEAccessKey) || string.IsNullOrEmpty(_settings.SCEAPIKey) ||
                string.IsNullOrEmpty(_settings.SCEAPISecret))
            {
                throw new ArgumentNullException("API fields have to be filled!");
            }
            var client = new sceApi();
            var auth = new AuthHeaderAPI
            {
                ApiAccessKey = _settings.SCEAccessKey,
                ApiKey = _settings.SCEAPIKey,
                ApiSecretKey = _settings.SCEAPISecret
            };
            client.AuthHeaderAPIValue = auth;
            return client;
        }

        //char separator = (char)0x0adf;
        private const char Separator = ',';

        private string LoadSceFile(string fileName)
        {
            var client = GetApiClient();
            client.Timeout = 90000000;
            var csvFile = GetLocalPath(fileName);
            var zipFile = csvFile.Replace(".csv", ".zip");

            _log.Warn("Downloading file: " + zipFile);
            var r = client.GetFullProductsExport(",");

                using (var fs = File.OpenWrite(zipFile))
                {
                    fs.Write(r, 0, r.Length);
                }

            using (var zipArc = ZipFile.Read(zipFile))
            {
                _log.Warn("Unzip File: " + zipFile);
                foreach (var z in zipArc)
                {
                    z.FileName = Path.GetFileName(csvFile);
                    z.Extract(Path.GetDirectoryName(csvFile), ExtractExistingFileAction.OverwriteSilently);
                    break;
                }
            }
            _log.Warn("Saved file: " + csvFile);
            return csvFile;
        }

        public List<SceProduct> LoadSceProducts(string fileName, out string path, out string zipPAth)
        {
            path = LoadSceFile(fileName);
            zipPAth = path.Replace(".csv", ".zip");
            return ReadSceProducts(path);
        }

        public List<SceProduct> ReadSceProducts(string fileName)
        {
            var products = new List<SceProduct>();
            using (var sr = File.OpenText(fileName))
            {
                using (var csv = new CsvReader(sr, true, Separator))
                {
                    var lastId = 0;
                    csv.GetFieldHeaders();
                    
                    while (csv.ReadNextRecord())
                    {
                        var id = int.Parse(csv["Prodid"]);
                        if (id != lastId)
                        {
                            var curProduct = new SceProduct
                            {
                               ProductId = id,
                                ProductType = int.Parse(csv["Product Type"]),
                                PartNumber = csv["Part Number"].RemoveAt().Trim(),
                                CostPrice = csv["Cost Price"].ParsePrice(),
                                WebPrice = csv["Web Price"].ParsePrice(),
                                MSRP = csv["MSRP"].ParsePrice(),
                                Brand = csv["Brand"],
                                Category = csv["Main Category"]
                            };
                            products.Add(curProduct);
                            lastId = id;
                        }
                    }
                }
            }
            return products;
        }

        private string GetLocalPath(string fileName)
        {
            var date = string.Format("_{0:dd-MM-yyyy_HH-mm}", DateTime.Now);
            return Path.Combine(
                !string.IsNullOrEmpty(_settings.LocalPath) ? _settings.LocalPath : Path.GetTempPath(),
                fileName + date + ".csv");
        }

        public List<string> GetUniqueFieldValues(List<SceProduct> products, string propName)
        {
            var fieldList = new List<string>();
            foreach (
                var propVal in
                    products.Select(prod => prod.GetPropValue(propName).ToString())
                        .Where(propVal => !fieldList.Contains(propVal)))
            {
                fieldList.Add(propVal);
            }

            return fieldList;
        }
    }
}
