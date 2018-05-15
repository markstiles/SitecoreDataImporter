using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {

    /// <summary>
    /// this is used to set a field to a specific predetermined value when importing data.
    /// </summary>
    public class ToStaticValue : BaseMapping, IBaseField {

        #region Properties

		///<summary>
		/// Value
		/// </summary>
        /// <value>
        /// value to import
        /// </value>
        public string Value { get; set; }

        #endregion Properties

        #region Constructor

        public ToStaticValue(Item i, ILogger l) : base(i)
		{
            Value = GetItemField(i, "Value");
        }

        #endregion Constructor

        #region IBaseField

        public string Name { get; set; }

        public void FillField(IDataMap map, ref Item newItem, string importValue) {
			Assert.IsNotNull(newItem, "newItem");
            //ignore import value and store value provided
            Field f = newItem.Fields[NewItemField];
            if (f != null)
                f.Value = Value;
        }

        /// <summary>
        /// doesn't provide any existing fields
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetExistingFieldNames() {
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// doesn't need any delimiter
        /// </summary>
        /// <returns></returns>
        public string GetFieldValueDelimiter() {
            return string.Empty;
        }

        #endregion IBaseField
    }
}
