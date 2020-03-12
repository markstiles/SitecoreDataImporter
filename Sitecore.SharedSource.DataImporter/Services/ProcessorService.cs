using HtmlAgilityPack;
using Sitecore.Data.Items;
using Sitecore.Reflection;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Processors.Pre;
using Sitecore.SharedSource.DataImporter.Processors.Field;
using Sitecore.SharedSource.DataImporter.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Services
{
    public class ProcessorService
    {
        protected ILogger Logger { get; set; }

        public ProcessorService(ILogger logger)
        {
            Logger = logger;
        }

        public virtual void ExecuteFieldProcessor(UrlImportMap urlImportMap, Item processor, Item itemToProcess, Item fieldMapping)
        {
            string handler = processor.Fields["Handler Class"].Value;
            string assembly = processor.Fields["Handler Assembly"].Value;
            var obj = ReflectionUtil.CreateObject(assembly, handler, new object[] { urlImportMap, Logger });

            var isProc = obj is IFieldProcessor;
            if (!isProc)
                return;

            var p = (IFieldProcessor)obj;
            p.Run(processor, itemToProcess, fieldMapping);
        }

        public virtual string ExecutePreProcessor(UrlImportMap urlImportMap, Item processor, HtmlDocument doc, string currentDirURL, string defaultTemplateID)
        {
            string handler = processor.Fields["Handler Class"].Value;
            string assembly = processor.Fields["Handler Assembly"].Value;
            var obj = ReflectionUtil.CreateObject(assembly, handler, new object[] { urlImportMap, Logger });

            var isProc = obj is IPreProcessor;
            if (!isProc)
                return "";
            
            var p = (IPreProcessor)obj;
            var value = p.Run(processor, doc, currentDirURL, defaultTemplateID);
               
            return value.ToString();
        }
    }
}