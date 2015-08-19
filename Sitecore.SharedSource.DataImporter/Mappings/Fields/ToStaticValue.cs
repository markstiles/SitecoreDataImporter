using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields 
{
    /// <summary>
    /// this is used to set a field to a specific predetermined value when importing data.
    /// </summary>
    
    public class ToStaticValue : BaseMapping, IBaseField 
    {
		#region Properties

		private string _Value;
		/// <summary>
		/// value to import
		/// </summary>
        public string Value {
			get {
				return _Value;
			}
			set {
				_Value = value;
			}
		}

		#endregion Properties

		#region Constructor

		public ToStaticValue(Item i) : base(i) {
			Value = i.Fields["Value"].Value;
		}

		#endregion Constructor

		#region Methods

        public void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            //ignore import value and store value provided
            Field f = newItem.Fields[NewItemField];
            if(f != null)
                f.Value = Value;
		}

		#endregion Methods

        #region IBaseField Methods

        /// <summary>
        /// doesn't provide any existing fields
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetExistingFieldNames()
        {
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// doesn't need any delimiter
        /// </summary>
        /// <returns></returns>
        public string GetFieldValueDelimiter()
        {
            return string.Empty;
        }

        #endregion IBaseField Methods
	}
}