using Sitecore.Data.Items;
using System;
using HtmlAgilityPack;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Processors.Pre
{
    public class SetTemplate : IPreProcessor
    {
        protected UrlImportMap UrlImportMap;
        protected ILogger Logger;
        protected HtmlService HtmlService;

        public SetTemplate(UrlImportMap urlImportMap, ILogger logger)
        {
            UrlImportMap = urlImportMap;
            Logger = logger;
            HtmlService = new HtmlService(logger);
        }

        public string Run(Item processor, HtmlDocument doc, string currentDirURL, string defaultTemplateID)
        {
            string identifier = processor.Fields["Identifier"].Value;
            HtmlNode node = HtmlService.HandleNodesLookup(identifier, doc);
            string action = node != null
                ? processor.Fields["Action"].Value
                : string.Empty;

            Logger.Log($"Template change From: {defaultTemplateID} To: {action}", processor.Paths.FullPath, Providers.ProcessStatus.Info);

            return action;
        }
    }
}
