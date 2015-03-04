using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Tests.DataMaps {
    public class EmptyItemNameDataMap : UTDataMap {
        public override string BuildNewItemName(object importRow) {
            return string.Empty;
        }
    }
}
