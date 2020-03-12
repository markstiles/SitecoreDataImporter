using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Processors.Models;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Processors.Pre
{
    public class DOMWarnings : IPreProcessor
    {
        protected UrlImportMap UrlImportMap;
        protected ILogger Logger;

        public string DomCheckStrings = "Warning Tags";

        public DOMWarnings(UrlImportMap urlImportMap, ILogger logger)
        {
            UrlImportMap = urlImportMap;
            Logger = logger;
        }

        public string Run(Item processor, HtmlDocument doc, string currentDirURL, string defaultTemplateID)
        {
            var warnings = GetDomCheckStrings(processor);
            foreach (ComplexWarningTag tag in warnings)
            {
                foreach (string searchText in tag.Identifiers)
                {
                    if (!doc.DocumentNode.OuterHtml.Contains(searchText))
                        continue;

                    Logger.Log($"DOM Warnings: {tag.Action} on {currentDirURL}", processor.Paths.FullPath, Providers.ProcessStatus.Info);
                }
            }

            return "";
        }
        
        private List<ComplexWarningTag> GetDomCheckStrings(Item processor)
        {
            List<ComplexWarningTag> warningTags = new List<ComplexWarningTag>();
            MultilistField warnings = processor.Fields[DomCheckStrings];
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
