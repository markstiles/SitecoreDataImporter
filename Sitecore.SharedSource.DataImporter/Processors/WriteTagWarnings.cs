using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Reporting;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class WriteTagWarnings
    {
        public static void Run(Item itemToProcess, Item fieldMapping, ILogger l)
        {
            MultilistField WarningTags = fieldMapping.Fields["Warning Trigger Tags"];
            ItemReport itemReport;

            if (!ImportReporter.ItemReports.ContainsKey(itemToProcess.Paths.Path))
                ImportReporter.ItemReports.Add(itemToProcess.Paths.Path, new ItemReport());

            ImportReporter.ItemReports.TryGetValue(itemToProcess.Paths.Path, out itemReport);
            if(string.IsNullOrEmpty(itemReport.ItemName))
                itemReport.ItemName = itemToProcess.Name;
            
            if (string.IsNullOrEmpty(itemReport.NewItemPath))
                itemReport.NewItemPath = itemToProcess.Paths.Path;
            
            BaseMapping baseMap = new BaseMapping(fieldMapping, l);
            List<string> tagsToCheck = new List<string>();
            HtmlDocument document = new HtmlDocument();
            string content = itemToProcess.Fields[baseMap.NewItemField].Value;
            document.LoadHtml(content);
            string findPattern;
            HtmlNode node;

            foreach (var id in WarningTags.TargetIDs)
            {
                findPattern = itemToProcess.Database.GetItem(id).Fields["Identifier"].Value;
                node = Helper.HandleNodesLookup(findPattern, document);
                if (node == null || node.ChildNodes == null || !node.ChildNodes.Any())
                    continue;

                tagsToCheck.Add(findPattern);
            }

            if (!tagsToCheck.Any())
                return;
            
            string fieldName = fieldMapping.Fields["To What Field"].Value;
            string messageText = string.Join("/", tagsToCheck.ToArray()) + " warning tags were found and requires additional review";

            ImportReporter.Write(itemToProcess, Level.Warning, messageText, fieldName, "Warning Trigger Tags");
        }
    }
}
