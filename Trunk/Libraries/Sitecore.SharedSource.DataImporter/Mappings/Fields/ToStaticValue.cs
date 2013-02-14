using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
	public class ToStaticValue : BaseField {

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

		//constructor
		public ToStaticValue(Item i) : base(i) {
			Value = i.Fields["Value"].Value;
		}

		#endregion Constructor

		#region Methods

		public override void FillField(ref Item newItem, DataRow importRow) {
			FillField(ref newItem);
		}

		public override void FillField(ref Item newItem, Item importRow) {
			FillField(ref newItem);
		}

		protected virtual void FillField(ref Item newItem) {
			newItem.Fields[NewItemField].Value = Value;
		}

		#endregion Methods
	}
}
