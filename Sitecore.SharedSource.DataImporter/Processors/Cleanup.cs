﻿using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.SharedSource.DataImporter.Reporting;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class Cleanup
    {
        public void Run(Item processor, Item itemToProcess, Item fieldMapping, ILogger l)
        {
            BaseMapping baseMap = new BaseMapping(fieldMapping, l);
            string findPattern = processor.Fields["Find"].Value;
            string replacePattern = processor.Fields["Replace"].Value;
            string replaceReportingText = string.IsNullOrEmpty(replacePattern) ? "*remove*" : replacePattern;


            try
            {
                
                HtmlDocument document = new HtmlDocument();
                string content = itemToProcess.Fields[baseMap.NewItemField].Value;
                document.LoadHtml(content);

                HtmlNode node = Helper.HandleNodesLookup(findPattern, document);

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
                        ImportReporter.Write(itemToProcess, Level.Info, string.Format("Replaced '{0}' with '{1}'", findPattern, replaceReportingText), baseMap.NewItemField, "Cleanup");
                    }
                    else
                    {
                        bool occuruncesFound = itemToProcess.Fields[baseMap.NewItemField].Value.Contains(findPattern);
                        itemToProcess.Fields[baseMap.NewItemField].Value = content.Replace(findPattern, replacePattern);
                        if (occuruncesFound)
                        {
                            ImportReporter.Write(itemToProcess, Level.Info, string.Format("Replaced '{0}' with '{1}'", findPattern, replaceReportingText), baseMap.NewItemField, "Cleanup");
                        }
                    }

                    itemToProcess.Editing.EndEdit(false, false);
                }
            }
            catch(Exception ex)
            {
                ImportReporter.Write(itemToProcess, Level.Info, string.Format("Replace failed on '{0}' with '{1}'", findPattern, replaceReportingText), baseMap.NewItemField, "Cleanup");
            }
        }
    }
}
