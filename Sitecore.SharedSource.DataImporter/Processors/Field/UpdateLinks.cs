using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Services;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Processors.Field
{
    public class UpdateLinks : IFieldProcessor
    {
        protected UrlImportMap UrlImportMap;
        protected ILogger Logger;
        protected HtmlService HtmlService;

        public UpdateLinks(UrlImportMap urlImportMap, ILogger logger)
        {
            UrlImportMap = urlImportMap;
            Logger = logger;
            HtmlService = new HtmlService(logger);
        }

        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            string fieldName = processor.Fields["Field Name"].Value;
            string domains = processor.Fields["Internal Domains"].Value;
            domains.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> internalDomains = domains.Split(new[] { '\n' }).ToList();

            UpdateFields(UrlImportMap.ImportLocation, fieldName, internalDomains, processor);
        }
        
        public void UpdateFields(Item importLocation, string fieldName, List<string> internalDomains, Item processor)
        {
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                Item[] items = UrlImportMap.MaintainHierarchy ? importLocation.Axes.GetDescendants() : importLocation.Children.ToArray();
                Item linkItem = null;

                foreach (var item in items)
                {
                    HtmlDocument doc = new HtmlDocument();
                    string fieldContent = "";
                    if (item.Fields[fieldName] != null)
                    {
                        fieldContent = item.Fields[fieldName].Value;
                        doc.LoadHtml(fieldContent);
                    }

                    if (string.IsNullOrEmpty(fieldContent)) { continue; }

                    foreach (HtmlNode node in doc.DocumentNode.Descendants("a"))
                    {
                        string url = node.Attributes["href"].Value.ToLower();
                        string olderValue = url;
                        string newValue = string.Empty;
                        string internalPath = string.Empty;
                        string urls = string.Empty;
                        string rootPath = string.Empty;

                        if (url.StartsWith("#")) { continue; }

                        if (url.Contains("."))
                        {
                            url = url.Remove(url.IndexOf('.'));
                        }

                        if (UrlImportMap.ExcludeDirectories != null && UrlImportMap.ExcludeDirectories.Any())
                        {
                            foreach (var dir in UrlImportMap.ExcludeDirectories)
                            {
                                string excludeDir = "/" + dir.ToLower();

                                if (url.Contains(excludeDir))
                                {
                                    url = url.Replace(excludeDir, "");
                                }
                            }

                            foreach (var domain in internalDomains)
                            {
                                if (url.ToLower().Contains(domain.ToLower()))
                                {
                                    url = url.Replace(domain, "");
                                }
                            }
                        }

                        if (url.StartsWith("http://")) { continue; }

                        urls = url;
                        List<string> directories = urls.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (UrlImportMap.MaintainHierarchy)
                        {

                            try
                            {
                                Uri baseURi = new Uri(UrlImportMap.BaseUrl);
                                rootPath = UrlImportMap.ImportLocation.Paths.FullPath + "/" + HtmlService.RemoveInvalidChars(baseURi.Host, true);
                                Item rootItem = UrlImportMap.ImportLocation.Database.GetItem(rootPath);

                                if (rootItem == null)
                                {
                                    foreach (var domain in internalDomains)
                                    {
                                        rootPath = UrlImportMap.ImportLocation.Paths.FullPath + "/" + HtmlService.RemoveInvalidChars(domain, true);
                                        rootItem = UrlImportMap.ImportLocation.Database.GetItem(rootPath);

                                        if (rootItem != null) { break; }
                                    }
                                }

                                rootPath = rootItem != null ? rootItem.Paths.FullPath : UrlImportMap.ImportLocation.Children.FirstOrDefault().Paths.FullPath;

                                internalPath = UrlImportMap.IgnoreRootDirectories ? UrlImportMap.ImportLocation.Paths.FullPath + "/"
                                    : rootPath + "/";
                            }
                            catch {
                                internalPath = UrlImportMap.IgnoreRootDirectories ? UrlImportMap.ImportLocation.Paths.FullPath + "/"
                                    : UrlImportMap.ImportLocation.Children.FirstOrDefault().Paths.FullPath + "/";
                            }
                            
                            foreach (var dir in directories)
                            {
                                internalPath += HtmlService.RemoveInvalidChars(dir, false);

                                if (dir != directories.LastOrDefault())
                                {
                                    internalPath += "/";
                                }
                            }

                            linkItem = UrlImportMap.ImportLocation.Database.GetItem(internalPath);
                        }
                        else
                        {
                            string itemName = HtmlService.RemoveInvalidChars(directories.LastOrDefault(), false);
                            internalPath = UrlImportMap.ImportLocation.Paths.FullPath + "/" + itemName;
                            linkItem = UrlImportMap.ImportLocation.Database.GetItem(internalPath);
                        }

                        if (linkItem != null)
                        {
                            newValue = "~/link.aspx?_id=" + linkItem.ID.ToShortID() + "&amp;_z=z";
                            node.Attributes["href"].Value = newValue;
                            Logger.Log($"Update Links: From: {olderValue} To: {newValue}", item.Paths.FullPath, Providers.ProcessStatus.Info, fieldName);
                        }
                    }

                    item.Editing.BeginEdit();
                    item.Fields[fieldName].Value = doc.DocumentNode.InnerHtml;
                    item.Editing.EndEdit();
                }
            }
        }
    }
}
