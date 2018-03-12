using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class DOMWarnings
    {
        public string Run(Item processor, HtmlDocument doc, string currentDirURL, string defaultTemplateID)
        {

            List<string> checkStrings = new List<string>();

            string messages = string.Empty;
            var warnings = GetDomCheckStrings(processor);
            foreach (ComplexWarningTag tag in warnings)
            {
                foreach (string searchText in tag.Identifiers)
                {
                    if (doc.DocumentNode.OuterHtml.Contains(searchText))
                    {
                        
                        ImportReporter.Write(processor, Level.Warning, tag.Action, "HTML Source", "DOM Parsing", currentDirURL);
                    }
                }
            }

            return "";
        }


        private List<ComplexWarningTag> GetDomCheckStrings(Item processor)
        {
            List<ComplexWarningTag> warningTags = new List<ComplexWarningTag>();
            MultilistField warnings = processor.Fields[Sitecore.SharedSource.DataImporter.HtmlScraper.Constants.FieldNames.DomCheckStrings];
            foreach (var id in warnings.TargetIDs)
            {
                var map = processor.Database.GetItem(id);
                ComplexWarningTag tag = new ComplexWarningTag();
                tag.Identifiers = map.Fields["Identifier"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                tag.Identifiers.RemoveAll(i => i == " ");
                tag.Action = map.Fields["Action"].Value;
                warningTags.Add(tag);
            }
            return warningTags;
        }

    }
}
