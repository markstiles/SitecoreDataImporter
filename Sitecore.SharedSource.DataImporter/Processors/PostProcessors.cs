using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.HtmlScraper;
using Sitecore.SharedSource.DataImporter.Mappings.Processors;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Processors
{
    public class PostProcessors : IPostProcessor
    {
        public void Process(IDataMap map)
        {
            RunPostProcessors(map);

            try {
                ImportReporter.Print();
            }
            catch {
            }
        }

        private void RunPostProcessors(IDataMap map)
        {
            ImportConfig config = new ImportConfig(map.ProviderItem, map.ToDB, map.Query);
            config.ImportLocation = map.ImportToWhere;

            foreach (var processor in config.PostProcessors)
            {
                Processor.Execute(processor.ProcessItem, config);
            }
        }

    }
}