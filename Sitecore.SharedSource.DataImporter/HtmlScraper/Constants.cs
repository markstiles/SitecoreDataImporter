using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class Constants
    {
        public struct Templates {

            public const string ProviderID = "{8C445567-92CB-4D7E-8EC1-C91667062211}";
        }
        public struct FieldNames
        {
            public const string ImportTextOnly = "Import Text Only";
            public const string IgnoreRootDirectories = "Ignore Root Directories";
            public const string MaintainHierarchy = "Maintain Hierarchy";
            public const string FromWhatField = "From What Fields";
            public const string ToWhatField = "To What Field";
            public const string UseXpath = "Use XPath For From";
            public const string AllowedExtensions = "Allowed URL Extensions";
            public const string URLCount = "Top x URLs";
            public const string ExcludeDirectories = "Exclude Directories";
            public const string BaseUrl = "Base URL";
            public const string DomCheckStrings = "Warning Tags";
            public const string ItemNameCleanups = "Item Name Cleanups";
            public const string PreProcessors = "Pre Processors";
            public const string PostProcessors = "Post Processors";
            public const string FieldPostProcessors = "Field Post Processors";
            public const string WarningTriggerTags = "Warning Trigger Tags";


        }
    }
}
