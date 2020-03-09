using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.SharedSource.DataImporter.Areas.DataImport.Models
{
    public class JobViewModel
    {
        public string Name { get; set; }
        public string Category { get;set; }
        public string State { get; set; }
        public string Processed { get; set; }
        public string Total { get; set; }
        public string Priority { get; set; }
        public string QueueTime { get; set; }
    }
}