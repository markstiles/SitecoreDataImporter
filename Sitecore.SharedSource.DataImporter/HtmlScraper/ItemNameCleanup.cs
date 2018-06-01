using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.HtmlScraper
{
    public class ItemNameCleanup
    {
        public string Find { get; set; }
        public string Replace { get; set; }
        public Item CleanupItem { get; set; }
    }
}
