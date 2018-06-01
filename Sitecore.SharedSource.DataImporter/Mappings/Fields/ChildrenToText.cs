using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
    /// <summary>
    /// this stores the plain text import value as is into the new field
    /// </summary>
    public class ChildrenToText : BaseMapping, IBaseProperty
    {
        #region Constructor

        public ChildrenToText(Item i, ILogger l) : base(i) { }

        #endregion Constructor

        #region IBaseField

        public string Name { get; set; }

        public void FillField(IDataMap map, ref Item newItem, Item importRow)
        {
            if (importRow.Children == null)
                return;
            
            var text = string.Join("|", importRow.Children.Select(x => x.ID.ToString()));
            if (string.IsNullOrEmpty(text))
                return;
            
            Field f = newItem.Fields[NewItemField];
            if (f != null)
                f.Value = text;
        }
        
        #endregion IBaseField
    }
}
