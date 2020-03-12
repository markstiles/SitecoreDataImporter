using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Processors;
using Sitecore.SharedSource.DataImporter.Processors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Processors.Models
{
    public class PostProcessors : Process
    {
        public string Identifier { get; set; }
        public string Action { get; set; }
        public Item ProcessItem { get; set; }
    }
}
