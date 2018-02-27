using System;
using System.Collections.Generic;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
    /// <summary>
    /// this stores the plain text import value as is into the new field
    /// </summary>
    public class ToText : BaseMapping, IBaseField {

        #region Properties

        /// <summary>
        /// name field delimiter
        /// </summary>
        protected readonly char[] comSplitr = { ',' };

		///<summary>
		/// ExistingDataNames
		/// </summary>
        /// <value>
        /// the existing data fields you want to import
        /// </value>
        public IEnumerable<string> ExistingDataNames { get; set; }

        /// <summary>
        /// Delimiter
        /// </summary>
        /// <value>the delimiter you want to separate imported data with</value>
        public string Delimiter { get; set; }

        #endregion Properties

        #region Constructor

        public ToText(Item i, ILogger l) : base(i)
		{
            //store fields
            ExistingDataNames = GetItemField(i, "From What Fields").Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
            Delimiter = GetItemField(i, "Delimiter");
        }

        #endregion Constructor

        #region IBaseField

        public string Name { get; set; }

        public virtual void FillField(IDataMap map, ref Item newItem, string importValue) {

			Assert.IsNotNull(newItem, "newItem");
            if (string.IsNullOrEmpty(importValue))
                return;

            //store the imported value as is
            Field f = newItem.Fields[NewItemField];
            if (f != null)
                f.Value = importValue.Trim();
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
