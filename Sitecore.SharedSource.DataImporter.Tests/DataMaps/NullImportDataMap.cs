using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DataImporter.Tests.DataMaps {
    public class NullImportDataMap : UTDataMap {
        public override IEnumerable<object> GetImportData() {
            throw new Exception("NullImportDataMap GetImportData() exception");
        }
    }
}
