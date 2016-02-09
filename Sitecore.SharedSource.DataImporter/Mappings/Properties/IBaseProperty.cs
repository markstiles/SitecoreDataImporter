using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Properties {
    public interface IBaseProperty : IBaseMapping {

        #region Methods

        string Name { get; set; }

        void FillField(IDataMap map, ref Item newItem, Item importRow);

        #endregion Methods
    }
}
