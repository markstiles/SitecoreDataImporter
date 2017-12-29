using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Collections.Specialized;
using System.Xml.Linq;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class ImportConfig
    {
        public string Name { get; set; }
        public Item ImportLocation { get; set; }
        public bool IgnoreRootDirectories { get; set; }
        public bool MaintainHierarchy { get; set; }
        public bool ImportTextOnly { get; set; }
        public List<string> AllowedExtensions { get; set; }
        public List<string> ExcludeDirectories { get; set; }
        public string URLCount { get; set; }
        //public List<ImportMappings> ImportMappings { get; set; }

        public Item SelectedMapping { get; set; }
        public List<string> StoredURLs { get; set; }

        public ImportConfig()
        {

        }

        protected bool IsSiteMapURL(string url)
        {
            return url.EndsWith(".xml");
        }

        public ImportConfig(Item config, Database db, string query)
        {

            if (config != null)
            {

                AllowedExtensions = new List<string>();
                ExcludeDirectories = new List<string>();
                Name = config.Name;
                int count = 0;
                StoredURLs = new List<string>();
                IgnoreRootDirectories = config.Fields[Constants.FieldNames.IgnoreRootDirectories].Value == "1" ? true : false;
                MaintainHierarchy = config.Fields[Constants.FieldNames.MaintainHierarchy].Value == "1" ? true : false;
                ImportTextOnly = config.Fields[Constants.FieldNames.ImportTextOnly].Value == "1" ? true : false;
                AllowedExtensions = config.Fields[Constants.FieldNames.AllowedExtensions].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
               
                URLCount = config.Fields[Constants.FieldNames.URLCount].Value;
                ExcludeDirectories = config.Fields[Constants.FieldNames.ExcludeDirectories].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                SelectedMapping = config;
                string storedValues = query;// config.Fields["URLs"].Value;
                storedValues.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> urls = storedValues.Split(new[] {'\n'}).ToList();
                urls = urls.Where(u => !u.StartsWith("//")).ToList();
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

                StoredURLs = StoredURLs.Select(u => u.TrimEnd('\r', '\n').ToLower()).ToList();

                if (AllowedExtensions != null && AllowedExtensions.Any())
                {
                    StoredURLs = StoredURLs.Where(x => AllowedExtensions.Any(e => x.EndsWith(("." + e.Trim())))).ToList();
                }

                if (int.TryParse(URLCount, out count))
                {
                    StoredURLs = StoredURLs.Take(count).ToList();
                }
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
