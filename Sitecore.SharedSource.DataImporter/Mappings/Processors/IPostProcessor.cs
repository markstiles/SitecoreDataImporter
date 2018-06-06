using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Syndication;

namespace Sitecore.SharedSource.DataImporter.Mappings.Processors
{
    public interface IPostProcessor
    {
        void Process(IDataMap map);
    }
}