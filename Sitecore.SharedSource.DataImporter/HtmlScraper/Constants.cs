using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class Constants
    {
        public struct FieldNames
        {
            public const string ImportTextOnly = "Import Text Only";
            public const string IgnoreRootDirectories = "Ignore Root Directories";
            public const string MaintainHierarchy = "Maintain Hierarchy";
            public const string FromWhatField = "From What Fields";
            public const string ToWhatField = "To What Field";
            public const string AllowedExtensions = "Allowed URL Extensions";
            public const string URLCount = "Top x URLs";
            public const string ExcludeDirectories = "Exclude Directories";

        }
    }
}
