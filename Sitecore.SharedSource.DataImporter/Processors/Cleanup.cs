using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Mappings;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class Cleanup
    {
        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            string findPattern = processor.Fields["Find"].Value;
            string replacePattern = processor.Fields["Replace"].Value;
            BaseMapping baseMap = new BaseMapping(fieldMapping);
            
            HtmlDocument document = new HtmlDocument();
            string content = itemToProcess.Fields[baseMap.NewItemField].Value;
            document.LoadHtml(content);

            HtmlNode node = Helper.HandleNodesLookup(findPattern, document);

            using (new SecurityModel.SecurityDisabler())
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
                    else {
                        itemToProcess.Fields[baseMap.NewItemField].Value = content.Replace(node.OuterHtml, replacePattern);
                    }
                }
                else {
                    itemToProcess.Fields[baseMap.NewItemField].Value = content.Replace(findPattern, replacePattern);
                }

                itemToProcess.Editing.EndEdit();
            }

        }
    }
}
