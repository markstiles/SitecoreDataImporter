using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Data;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	public class ListToGuid : ToText {

		#region Properties

		private string _SourceList;
		public string SourceList {
			get {
				return _SourceList;
			}
			set {
				_SourceList = value;
			}
		}

		#endregion Properties

		#region Constructor

		//constructor
		public ListToGuid(Item i) : base(i) {
			SourceList = i.Fields["Source List"].Value;
		}

		#endregion Constructor

		#region Methods

		//fills it's own field
		public override void FillField(ref Item newItem, DataRow importRow) {
			FillField(ref newItem, GetValueFromDataRow(importRow));
		}

		public override void FillField(ref Item newItem, Item importRow) {
			FillField(ref newItem, GetValueFromItem(importRow));
		}

		protected override void FillField(ref Item newItem, string existingValue) {

			Item i = newItem.Database.GetItem(SourceList);
			if (i != null) {
				foreach (Item child in i.GetChildren()) {
					if (child.DisplayName.Equals(existingValue)) {
						newItem.Fields[NewItemField].Value = child.ID.ToString();
					}
				}
			}
		}

		#endregion Methods
	}
}
