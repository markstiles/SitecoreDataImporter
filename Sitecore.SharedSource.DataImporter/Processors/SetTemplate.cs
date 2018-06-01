using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlAgilityPack;
using System;
using Sitecore.SharedSource.DataImporter.Reporting;
using Sitecore.SharedSource.DataImporter.HtmlScraper;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class SetTemplate
    {
       
        public string Run(Item processor, HtmlDocument doc, string currentDirURL, string defaultTemplateID)
        {

            string identifier = processor.Fields["Identifier"].Value;
            string action = string.Empty;

            HtmlNode node = Helper.HandleNodesLookup(identifier, doc);

            if (node != null) {
                action = processor.Fields["Action"].Value;
            }

            ImportReporter.Write(processor, Level.Info, "Template change From: " + defaultTemplateID + " To: " + action, "N/A", "Template Change", itemFetchURL: currentDirURL);

            return action;
        }
    }
}
