using System;
using System.Globalization;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Logger;
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
        public ToDate(Item i, ILogger l) : base(i, l)
		{

        }

        #endregion Constructor

        #region IBaseField

        public override void FillField(IDataMap map, ref Item newItem, string importValue) {
            if (string.IsNullOrEmpty(importValue))
                return;

            //try to parse date value 
            DateTime date;
            if (!DateTime.TryParse(importValue, out date)
                && !DateTime.TryParseExact(importValue, new string[] { "yyyyMMdd", "d/M/yyyy", "d/M/yyyy HH:mm:ss", "yyyyMMddTHHmmss", "yyyyMMddTHHmmssZ" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))  {
                map.Logger.Log("ToDate.FillField", string.Format("Date parse error for date {0} on item {1}", importValue, newItem.Paths.FullPath));
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
