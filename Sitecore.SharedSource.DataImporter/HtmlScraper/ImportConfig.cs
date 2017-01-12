using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class ImportConfig
    {
        public string Name { get; set; }
        public Item ImportLocation { get; set; }
        public bool IgnoreRootDirectories { get; set; }
        public bool EnableSmartDirectory { get; set; }
        public bool ImportTextOnly { get; set; }
        public List<ImportMappings> ImportMappings { get; set; }

        public Item SelectedMapping { get; set; }
        public List<string> StoredURLs { get; set; }

        public ImportConfig()
        {

        }

        protected bool IsSiteMapURL(string url)
        {
            return url.EndsWith(".xml");
        }

        public ImportConfig(Item config, Database db)
        {

            if (config != null)
            {
                Name = config.Name;
                StoredURLs = new List<string>();
                IgnoreRootDirectories = config.Fields["Ignore Root Directories"].Value == "1" ? true : false;
                EnableSmartDirectory = config.Fields["Maintain Hierarchy"].Value == "1" ? true : false;
                ImportTextOnly = config.Fields["Import Text Only"].Value == "1" ? true : false;
                SelectedMapping = config;
                string storedValues = config.Fields["URLs"].Value;
                storedValues.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> urls = storedValues.Split(new[] {'\n'}).ToList();
                List<string> sitemapURLs = urls.Where(u => IsSiteMapURL(u.Trim())).ToList();
                List<string> pageURLs = urls.Where(u => !IsSiteMapURL(u.Trim())).ToList();

                if (sitemapURLs.Any())
                {
                    ImportFromSitemap(sitemapURLs, StoredURLs);
                }

                if (pageURLs.Any())
                {
                    StoredURLs.AddRange(pageURLs);
                }

                ImportMappings = new MapData(config, db).MappedData;
            }
        }


        private void ImportFromSitemap(List<string> sitemapURLs, List<string> storedList) 
        {
            foreach (var s in sitemapURLs)
            {

                XDocument xdoc = XDocument.Load(s);
                var ns = xdoc.Root.Name.Namespace;
                List<XElement> xLocs = xdoc.Root.Elements(ns + "url").Elements(ns + "loc").ToList();
                List<string> urls = xLocs.Select(u => u.Value).ToList();

                storedList.AddRange(urls);
            }
        }

    }
}
