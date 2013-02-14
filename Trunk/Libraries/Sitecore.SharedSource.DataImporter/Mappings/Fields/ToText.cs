using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.Data.Items;
using System.Web;
using Sitecore.Data.Fields;
using System.Data;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	public class ToText : BaseField {
		
		#region Properties 

		public char[] comSplitr = { ',' };

		private string _existingDataName;
		public string ExistingDataName {
			get {
				return _existingDataName;
			}
			set {
				_existingDataName = value;
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

		//constructor
		public ToText(Item i) : base(i) {
			
			ExistingDataName = i.Fields["From What Fields"].Value;
			Delimiter = i.Fields["Delimiter"].Value;
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
				newItem.Fields[NewItemField].Value = existingValue;
			} catch (Exception ex) {
				//this is because the value was not a proper date
				HttpContext.Current.Response.Write(newItem.Paths.Path + " - " + NewItemField + "<br/>");
			}
		}

		public string GetValueFromDataRow(DataRow importItem) {

			
			string[] existingDataNames = ExistingDataName.Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			string fullData = "";
			int i = 0;
			foreach (string dataName in existingDataNames) {
				if (i > 0) {
					fullData += Delimiter;
				}
				fullData += importItem[dataName].ToString();
				i++;
			}

			return fullData;
		}

		public string GetValueFromItem(Item importItem) {
			string[] existingDataNames = ExistingDataName.Split(comSplitr, StringSplitOptions.RemoveEmptyEntries);
			string fullData = "";
			int i = 0;
			foreach (string dataName in existingDataNames) {
				if (i > 0) {
					fullData += Delimiter;
				}
				fullData += importItem.Fields[dataName].Value;
				i++;
			}
			return fullData;
		}

		#endregion Methods
	}
}
