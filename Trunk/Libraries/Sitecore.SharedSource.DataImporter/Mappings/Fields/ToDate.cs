using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Extensions;
using System.Data;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	public class ToDate : ToText {

		#region Properties 

		#endregion Properties
		
		#region Constructor

		//constructor
		public ToDate(Item i) : base(i) {
			
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
			
			try {
				DateTime date = DateTime.Parse(existingValue);
				newItem.Fields[NewItemField].Value = date.ToDateFieldValue();
			} catch (Exception ex) {
				//this is because the value was not a proper date
			}
		}


		#endregion Methods
	}
}
