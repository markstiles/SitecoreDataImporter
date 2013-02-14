using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using System.Data;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	public class UrlToLink : ToText {
		#region Properties 

		#endregion Properties
		
		#region Constructor

		//constructor
		public UrlToLink(Item i)
			: base(i) {
			
		}

		#endregion Constructor
		
		#region Methods

		//methods
		public override void FillField(ref Item newItem, DataRow importRow) {
			FillField(ref newItem, GetValueFromDataRow(importRow));
		}

		public override void FillField(ref Item newItem, Item importRow) {
			FillField(ref newItem, GetValueFromItem(importRow));
		}

		protected virtual void FillField(ref Item newItem, string existingValue) {
			
			LinkField lf = newItem.Fields[NewItemField];
			lf.Url = existingValue;
		}

		#endregion Methods
	}
}
