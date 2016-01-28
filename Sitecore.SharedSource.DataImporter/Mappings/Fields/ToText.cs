using System;
using System.Collections.Generic;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
    /// <summary>
    /// this stores the plain text import value as is into the new field
    /// </summary>
    public class ToText : BaseMapping, IBaseField {

        #region Properties

        /// <summary>
        /// name field delimiter
        /// </summary>
        public char[] comSplitr = { ',' };

        /// <summary>
        /// the existing data fields you want to import
        /// </summary>
        public IEnumerable<string> ExistingDataNames { get; set; }

        /// <summary>
        /// the delimiter you want to separate imported data with
        /// </summary>
        public string Delimiter { get; set; }

        #endregion Properties

        #region Constructor

        public ToText(Item i)
            : base(i) {
            //store fields
            ExistingDataNames = GetItemField(i, "From What Fields").Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
            Delimiter = GetItemField(i, "Delimiter");
        }

        #endregion Constructor

        #region IBaseField

        public string Name { get; set; }

        public virtual void FillField(IDataMap map, ref Item newItem, string importValue) {

            if (string.IsNullOrEmpty(importValue))
                return;

            //store the imported value as is
            Field f = newItem.Fields[NewItemField];
            if (f != null)
                f.Value = importValue;
        }

        /// <summary>
        /// returns a string list of fields to import
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetExistingFieldNames() {
            return ExistingDataNames;
        }

        /// <summary>
        /// return the delimiter to separate imported values with
        /// </summary>
        /// <returns></returns>
        public string GetFieldValueDelimiter() {
            return Delimiter;
        }

        #endregion IBaseField
    }
}
