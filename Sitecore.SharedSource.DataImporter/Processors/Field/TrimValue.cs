using Sitecore.Data.Items;
using System;
using Sitecore.SharedSource.DataImporter.Mappings;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Processors.Field
{
    public class TrimValue : IFieldProcessor
    {
        protected UrlImportMap UrlImportMap;
        protected ILogger Logger;

        public TrimValue(UrlImportMap urlImportMap, ILogger logger)
        {
            UrlImportMap = urlImportMap;
            Logger = logger;
        }

        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
            BaseMapping baseMap = new BaseMapping(fieldMapping, Logger);
            try
            {
                using (new Sitecore.SecurityModel.SecurityDisabler())
                {
                    itemToProcess.Editing.BeginEdit();
                    itemToProcess.Fields[baseMap.NewItemField].Value = itemToProcess.Fields[baseMap.NewItemField].Value.Trim();
                    itemToProcess.Editing.EndEdit();
                }
                Logger.Log("Trim Value", itemToProcess.Paths.FullPath, Providers.ProcessStatus.Info, baseMap.NewItemField);
            }
            catch(Exception ex)
            {
                Logger.Log($"Trim Value: Error: {ex}", itemToProcess.Paths.FullPath, Providers.ProcessStatus.Error, baseMap.NewItemField);
            }
        }
    }
}
