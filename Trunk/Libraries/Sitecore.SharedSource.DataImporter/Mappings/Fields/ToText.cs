using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.Data.Items;
using System.Web;
using Sitecore.Data.Fields;
using System.Data;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
    public class ToText : BaseMapping, IBaseField
    {
		
		#region Properties 

		public char[] comSplitr = { ',' };

		private IEnumerable<string> _existingDataNames;
		public IEnumerable<string> ExistingDataNames {
			get {
				return _existingDataNames;
			}
			set {
				_existingDataNames = value;
			}
		}

		private string _delimiter;
		public string Delimiter {
			get {
				return _delimiter;
			}
			set {
				_delimiter = value;
			}
		}
		
		#endregion Properties
		
		#region Constructor

		public ToText(Item i) : base(i) {

            ExistingDataNames = i.Fields["From What Fields"].Value.Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			Delimiter = i.Fields["Delimiter"].Value;
		}

		#endregion Constructor
		
		#region Methods

		public virtual void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            Field f = newItem.Fields[NewItemField];
            if(f != null)
                f.Value = importValue;
		}

        #endregion Methods

        #region IBaseField Methods

        public string GetNewFieldName()
        {
            return NewItemField;
        }

        public IEnumerable<string> GetExistingFieldNames()
        {
            return ExistingDataNames;
        }

        public string GetFieldValueDelimiter()
        {
            return Delimiter;
        }

        #endregion IBaseField Methods
    }
}
