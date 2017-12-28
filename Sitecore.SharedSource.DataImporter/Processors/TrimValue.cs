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
    public class TrimValue
    {
        public void Run(Item processor, Item itemToProcess, Item fieldMapping)
        {
          
            BaseMapping baseMap = new BaseMapping(fieldMapping);

            using (new SecurityModel.SecurityDisabler())
            {
                itemToProcess.Editing.BeginEdit();
                itemToProcess.Fields[baseMap.NewItemField].Value = itemToProcess.Fields[baseMap.NewItemField].Value.Trim();
                itemToProcess.Editing.EndEdit();
            }
        }
    }
}
