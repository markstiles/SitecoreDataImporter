using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	public class ToStaticValue : BaseMapping, IBaseField {

		#region Properties

		private string _Value;
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
            Field f = newItem.Fields[NewItemField];
            if(f != null)
                f.Value = Value;
		}

		#endregion Methods

        #region IBaseField Methods

        public string GetNewFieldName()
        {
            return NewItemField;
        }

        public IEnumerable<string> GetExistingFieldNames()
        {
            return Enumerable.Empty<string>();
        }

        public string GetFieldValueDelimiter()
        {
            return string.Empty;
        }

        #endregion IBaseField Methods
	}
}
