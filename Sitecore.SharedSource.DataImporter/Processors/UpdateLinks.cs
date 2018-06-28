using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class UpdateLinks
    {
        public void Run(Item processor, ImportConfig config)
        {
            string fieldName = processor.Fields["Field Name"].Value;

            string domains = processor.Fields["Internal Domains"].Value;
            domains.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> internalDomains = domains.Split(new[] { '\n' }).ToList();
            UpdateFields(config.ImportLocation, config, fieldName, internalDomains, processor);
        }


        private void UpdateFields(Item importLocation, ImportConfig config, string fieldName, List<string> internalDomains, Item processor)
        {
            using (new SecurityModel.SecurityDisabler())
            {
                Item[] items = config.MaintainHierarchy ? importLocation.Axes.GetDescendants() : importLocation.Children.ToArray();
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

                        if (config.ExcludeDirectories != null && config.ExcludeDirectories.Any())
                        {
                            foreach (var dir in config.ExcludeDirectories)
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

                        if (config.MaintainHierarchy)
                        {

                            try
                            {
                                Uri baseURi = new Uri(config.BaseUrl);
                                rootPath = config.ImportLocation.Paths.FullPath + "/" + Helper.RemoveInvalidChars(config, baseURi.Host, true);
                                Item rootItem = config.ImportLocation.Database.GetItem(rootPath);

                                if (rootItem == null)
                                {
                                    foreach (var domain in internalDomains)
                                    {
                                        rootPath = config.ImportLocation.Paths.FullPath + "/" + Helper.RemoveInvalidChars(config, domain, true);
                                        rootItem = config.ImportLocation.Database.GetItem(rootPath);

                                        if (rootItem != null) { break; }
                                    }
                                }

                                rootPath = rootItem != null ? rootItem.Paths.FullPath : config.ImportLocation.Children.FirstOrDefault().Paths.FullPath;

                                internalPath = config.IgnoreRootDirectories ? config.ImportLocation.Paths.FullPath + "/"
                                    : rootPath + "/";
                            }
                            catch {
                                internalPath = config.IgnoreRootDirectories ? config.ImportLocation.Paths.FullPath + "/"
                                    : config.ImportLocation.Children.FirstOrDefault().Paths.FullPath + "/";
                            }
                            
                            foreach (var dir in directories)
                            {
                                internalPath += Helper.RemoveInvalidChars(config, dir, false);

                                if (dir != directories.LastOrDefault())
                                {
                                    internalPath += "/";
                                }
                            }

                            linkItem = config.ImportLocation.Database.GetItem(internalPath);
                        }
                        else
                        {
                            string itemName = Helper.RemoveInvalidChars(config, directories.LastOrDefault(), false);
                            internalPath = config.ImportLocation.Paths.FullPath + "/" + itemName;
                            linkItem = config.ImportLocation.Database.GetItem(internalPath);
                        }

                        if (linkItem != null)
                        {
                            newValue = "~/link.aspx?_id=" + linkItem.ID.ToShortID() + "&amp;_z=z";
                            node.Attributes["href"].Value = newValue;
                            ImportReporter.Write(item, Level.Info, "From: " + olderValue + " To: " + newValue, fieldName, "Link Update");
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
