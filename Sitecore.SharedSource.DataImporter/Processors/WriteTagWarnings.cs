using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Reporting;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class WriteTagWarnings
    {
        public static void Run(Item itemToProcess, Item fieldMapping)
        {
            MultilistField WarningTags = fieldMapping.Fields["Warning Trigger Tags"];
            ItemReport itemReport;

            if (!ImportReporter.ItemReports.ContainsKey(itemToProcess.Paths.Path))
            {
                ImportReporter.ItemReports.Add(itemToProcess.Paths.Path, new ItemReport());
            }
            if (!ImportReporter.ItemReports.TryGetValue(itemToProcess.Paths.Path,out itemReport))
            {
                //throw new NullReferenceException("Could not create the item report.");
            }

            if(string.IsNullOrEmpty(itemReport.ItemFetchPath))
            {
                //itemReport.ItemFetchPath;
            }

            if(string.IsNullOrEmpty(itemReport.ItemName))
            {
                itemReport.ItemName = itemToProcess.Name;
            }

            if (string.IsNullOrEmpty(itemReport.NewItemPath))
            {
                itemReport.NewItemPath = itemToProcess.Paths.Path;
            }

            BaseMapping baseMap = new BaseMapping(fieldMapping);
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
                if (node != null)
                {
                    if (node.ChildNodes != null && node.ChildNodes.Count > 0)
                    {
                        tagsToCheck.Add(findPattern);
                    }
                }
            }

            if(tagsToCheck.Count > 0)
            {
                string fieldName = fieldMapping.Fields["To What Field"].Value;
                string messageText = string.Join("/", tagsToCheck.ToArray()) + " warning tags were found and requires additional review";

                ImportReporter.Write(itemToProcess, Level.Warning, messageText, fieldName, "Warning Trigger Tags");
            }
        }
    }
}
