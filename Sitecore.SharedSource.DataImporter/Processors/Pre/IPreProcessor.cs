using HtmlAgilityPack;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Processors.Pre
{
    public interface IPreProcessor
    {
        string Run(Item processor, HtmlDocument doc, string currentDirURL, string defaultTemplateID);
    }
}