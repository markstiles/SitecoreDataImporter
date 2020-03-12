using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Processors.Field
{
    public class WriteTagWarnings : IFieldProcessor
    {
        protected UrlImportMap UrlImportMap;
        protected ILogger Logger;
        protected HtmlService HtmlService;

        public WriteTagWarnings(UrlImportMap urlImportMap, ILogger logger)
        {
            UrlImportMap = urlImportMap;
            Logger = logger;
            HtmlService = new HtmlService(logger);
        }
        
        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            BaseMapping baseMap = new BaseMapping(fieldMapping, Logger);
            string content = itemToProcess.Fields[baseMap.NewItemField].Value;
            var document = new HtmlDocument();
            document.LoadHtml(content);

            List<string> tagsToCheck = new List<string>();
            var WarningTags = (MultilistField)fieldMapping.Fields["Warning Trigger Tags"];
            foreach (var id in WarningTags.TargetIDs)
            {
                var findPattern = itemToProcess.Database.GetItem(id).Fields["Identifier"].Value;
                var node = HtmlService.HandleNodesLookup(findPattern, document);
                if (node == null || node.ChildNodes == null || !node.ChildNodes.Any())
                    continue;

                tagsToCheck.Add(findPattern);
            }

            if (!tagsToCheck.Any())
                return;
            
            string fieldName = fieldMapping.Fields["To What Field"].Value;

            Logger.Log($"Warning Trigger Tags: {string.Join("/", tagsToCheck.ToArray())} warning tags were found and requires additional review", itemToProcess.Paths.FullPath, Providers.ProcessStatus.Warning, fieldName);
        }
    }
}
