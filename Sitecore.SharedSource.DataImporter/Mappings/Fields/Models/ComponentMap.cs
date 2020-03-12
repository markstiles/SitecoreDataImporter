using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields.Models
{
    public class ComponentMap
    {
        public string FromWhatTemplate { get; set; }
        public string Component { get; set; }
        public string Placeholder { get; set; }
        public string DatasourcePath { get; set; }
        public bool OverwriteExisting { get; set; }
        public Dictionary<string, string> Fields { get; set; }
        public string Parameters { get; set; }
    }
}