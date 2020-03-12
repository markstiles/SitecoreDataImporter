using HtmlAgilityPack;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Processors.Field
{
    public interface IFieldProcessor
    {
        void Run(Item processor, Item itemToProcess, Item fieldMapping);
    }
}