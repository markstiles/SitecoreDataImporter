using HtmlAgilityPack;
using Sitecore.Data.Items;
using Sitecore.Reflection;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Processors;
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

        public virtual void ExecuteProcessor(IDataMap dataMap, Item processor, object importRow, Item newItem)
        {
            string handler = processor.Fields["Handler Class"].Value;
            string assembly = processor.Fields["Handler Assembly"].Value;
            var obj = ReflectionUtil.CreateObject(assembly, handler, new object[] { dataMap, processor, Logger });

            var isProc = obj is IProcessor;
            if (!isProc)
                return;

            var p = (IProcessor)obj;
            p.Run(importRow, newItem);
        }
    }
}