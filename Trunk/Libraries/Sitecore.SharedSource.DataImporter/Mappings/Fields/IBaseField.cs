using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	
	public interface IBaseField {

		#region Methods

        void FillField(BaseDataMap map, ref Item newItem, string importValue);

        string GetNewFieldName();

        IEnumerable<string> GetExistingFieldNames();

        string GetFieldValueDelimiter();

		#endregion Methods
	}
}
