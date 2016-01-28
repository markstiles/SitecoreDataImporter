using System;
using System.Globalization;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Providers;
namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {

    /// <summary>
    /// This field converts a date value to a sitecore date field value
    /// </summary>
    public class ToDate : ToText {

        #region Properties

        #endregion Properties

        #region Constructor

        //constructor
        public ToDate(Item i)
            : base(i) {

        }

        #endregion Constructor

        #region IBaseField

        public override void FillField(IDataMap map, ref Item newItem, string importValue) {
            if (string.IsNullOrEmpty(importValue))
                return;

            //try to parse date value 
            DateTime date;
            if (!DateTime.TryParse(importValue, out date)
                && !DateTime.TryParseExact(importValue, new string[] { "d/M/yyyy", "d/M/yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))  {
                map.Logger.LogError("DateTime parse error", string.Format("item '{0}', from '{1}', date '{2}'", newItem.Paths.FullPath, string.Join(Delimiter, ExistingDataNames), importValue));
                return;
            }
            
            Field f = newItem.Fields[NewItemField];
            if (f == null)
                return;

            f.Value = date.ToDateFieldValue();
        }

        #endregion IBaseField
    }
}
