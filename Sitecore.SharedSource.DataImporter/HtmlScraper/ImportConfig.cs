using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Collections.Specialized;
using System.Xml.Linq;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class ImportConfig
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public Item ImportLocation { get; set; }
        public bool IgnoreRootDirectories { get; set; }
        public bool MaintainHierarchy { get; set; }
        public bool ImportTextOnly { get; set; }
        public List<string> AllowedExtensions { get; set; }
        public List<ItemNameCleanup> ItemNameCleanups { get; set; }
        public List<PreProcessor> PreProcessors { get; set; }
        public List<PostProcessors> PostProcessors { get; set; }
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
                ItemNameCleanups = GetItemNameCleanups(config, db);
                PreProcessors = GetPreProcessors(config, db);
                PostProcessors = GetPostProcessors(config, db);
                BaseUrl = config.Fields[Constants.FieldNames.BaseUrl].Value;

                URLCount = config.Fields[Constants.FieldNames.URLCount].Value;
                ExcludeDirectories = config.Fields[Constants.FieldNames.ExcludeDirectories].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                SelectedMapping = config;
                string storedValues = !string.IsNullOrEmpty(query) ? query : config.Fields["Query"].Value;
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

                    if (pageURLs.Any(p => p.Contains("?")))
                    {
                        var queryPages = pageURLs;
                        queryPages = queryPages.Select(u => u.TrimEnd('\r', '\n').ToLower()).ToList();
                        queryPages = queryPages.Where(x => AllowedExtensions.Any(e => x.Contains(("." + e.Trim() + "?")))).ToList();
                        StoredURLs.AddRange(queryPages);
                    }

                }


                

                if (int.TryParse(URLCount, out count))
                {
                    StoredURLs = StoredURLs.Take(count).ToList();
                }
            }
        }

        private List<PostProcessors> GetPostProcessors(Item config, Database db)
        {
            List<PostProcessors> processors = new List<PostProcessors>();
            MultilistField processorItems = config.Fields[Constants.FieldNames.PostProcessors];
            foreach (var id in processorItems.TargetIDs)
            {
                var item = db.GetItem(id);
                PostProcessors processor = new PostProcessors();
                processor.Identifier = item.Fields["Identifier"] != null ? item.Fields["Identifier"].Value : string.Empty;
                processor.Action = item.Fields["Action"] != null ? item.Fields["Action"].Value : string.Empty;
                processor.Type = item.Fields["Type"] != null ? item.Fields["Type"].Value : string.Empty;
                processor.Method = item.Fields["Method"] != null ? item.Fields["Method"].Value : string.Empty;
                processor.ProcessItem = item;
                processors.Add(processor);
            }
            return processors;
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

      
        private List<ItemNameCleanup> GetItemNameCleanups(Item config, Database db)
        {
            List<ItemNameCleanup> cleanups = new List<ItemNameCleanup>();
            MultilistField cleanupItems = config.Fields[Constants.FieldNames.ItemNameCleanups];
            foreach(var id in cleanupItems.TargetIDs)
            {
                var cleanupItem = db.GetItem(id);
                ItemNameCleanup cleanup = new ItemNameCleanup();
                cleanup.Find = cleanupItem.Fields["Find"].Value;
                cleanup.Replace = cleanupItem.Fields["Replace"].Value;
                cleanup.CleanupItem = cleanupItem;
                cleanups.Add(cleanup);
            }
            return cleanups;
        }

        private List<PreProcessor> GetPreProcessors(Item config, Database db)
        {
            List<PreProcessor> processors = new List<PreProcessor>();
            MultilistField processorItems = config.Fields[Constants.FieldNames.PreProcessors];
            foreach (var id in processorItems.TargetIDs)
            {
                var item = db.GetItem(id);
                PreProcessor processor = new PreProcessor();
                processor.Identifier = item.Fields["Identifier"] != null ? item.Fields["Identifier"].Value : string.Empty;
                processor.Action = item.Fields["Action"] != null ? item.Fields["Action"].Value : string.Empty;
                processor.Type = item.Fields["Type"] != null ? item.Fields["Type"].Value : string.Empty;
                processor.Method = item.Fields["Method"] != null ? item.Fields["Method"].Value : string.Empty;
                processor.ProcessItem = item;
                processors.Add(processor);
            }
            return processors;
        }


    }
}
