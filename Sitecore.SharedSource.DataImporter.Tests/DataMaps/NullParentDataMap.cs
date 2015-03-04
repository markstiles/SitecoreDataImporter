using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Tests.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Tests.DataMaps {
    public class NullParentDataMap : UTDataMap {
        public override Item GetParentNode(object importRow, string newItemName) {
            return null;
        }
    }
}
