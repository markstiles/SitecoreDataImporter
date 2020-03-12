using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Processors.Field
{
    public class Cleanup : IFieldProcessor
    {
        protected UrlImportMap UrlImportMap;
        protected ILogger Logger;
        protected HtmlService HtmlService;

        public Cleanup(UrlImportMap urlImportMap, ILogger logger)
        {
            UrlImportMap = urlImportMap;
            Logger = logger;
            HtmlService = new HtmlService(logger);
        }

        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            BaseMapping baseMap = new BaseMapping(fieldMapping, Logger);
            string findPattern = processor.Fields["Find"].Value;
            string replacePattern = processor.Fields["Replace"].Value;
            string replaceReportingText = string.IsNullOrEmpty(replacePattern) ? "*remove*" : replacePattern;
            
            try
            {   
                HtmlDocument document = new HtmlDocument();
                string content = itemToProcess.Fields[baseMap.NewItemField].Value;
                document.LoadHtml(content);

                HtmlNode node = HtmlService.HandleNodesLookup(findPattern, document);

                using (new Sitecore.SecurityModel.SecurityDisabler())
                {
                    itemToProcess.Editing.BeginEdit();
                    if (node != null)
                    {
                        if (findPattern.Contains("/*") && node.ChildNodes != null)
                        {
                            foreach (var child in node.ChildNodes)
                            {
                                content = content.Replace(child.OuterHtml, replacePattern);
                                itemToProcess.Fields[baseMap.NewItemField].Value = content;
                            }
                        }
                        else
                        {
                            itemToProcess.Fields[baseMap.NewItemField].Value = content.Replace(node.OuterHtml, replacePattern);
                        }
                        Logger.Log($"Cleanup: Replaced '{findPattern}' with '{replaceReportingText}'", itemToProcess.Paths.FullPath, Providers.ProcessStatus.Info, baseMap.NewItemField);
                    }
                    else
                    {
                        bool occuruncesFound = itemToProcess.Fields[baseMap.NewItemField].Value.Contains(findPattern);
                        itemToProcess.Fields[baseMap.NewItemField].Value = content.Replace(findPattern, replacePattern);
                        if (occuruncesFound)
                        {
                            Logger.Log($"Replaced {occuruncesFound} '{findPattern}' with '{replaceReportingText}'", itemToProcess.Paths.FullPath, Providers.ProcessStatus.Info, baseMap.NewItemField);
                        }
                    }

                    itemToProcess.Editing.EndEdit(false, false);
                }
            }
            catch(Exception ex)
            {
                Logger.Log($"Cleanup: Replace failed on '{findPattern}' with '{replaceReportingText}'", itemToProcess.Paths.FullPath, Providers.ProcessStatus.Error, baseMap.NewItemField);
            }
        }
    }
}
