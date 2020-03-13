using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields.SitecoreFields
{
    /// <summary>
    /// this stores the plain text import value as is into the new field
    /// </summary>
    public class SubitemFieldToText : ToText
    {
        public string SubitemQuery { get; set; }
        
        #region Constructor

        public SubitemFieldToText(Item i, ILogger l) : base(i, l)
        {
            SubitemQuery = GetItemField(i, "Subitem Query");
        }

        #endregion Constructor

        #region IBaseField

        public override void FillField(IDataMap map, ref Item newItem, object importRow)
        {
            Field f = newItem.Fields[ToWhatField];
            if (f == null)
                return;

            if (string.IsNullOrWhiteSpace(SubitemQuery))
                return;
            
            var importItem = importRow is Item ? (Item)importRow : null;
            if (importItem == null)
                return;

            var queriedItems = importItem.Axes.SelectItems(SubitemQuery);
            var result = queriedItems
                .SelectMany(a => ExistingDataNames
                    .Select(b => GetValue(a, b))
                    .Where(c => !string.IsNullOrWhiteSpace(c)))
                .ToList();

            result.Insert(0, f.Value);
            
            f.Value = string.Join(Delimiter, result);
        }
        
        protected string GetValue(Item i, string fieldName)
        {
            var valueField = i.Fields[fieldName];
            if (valueField == null)
                return string.Empty;

            return valueField.Value;
        }

        #endregion IBaseField
    }
}
