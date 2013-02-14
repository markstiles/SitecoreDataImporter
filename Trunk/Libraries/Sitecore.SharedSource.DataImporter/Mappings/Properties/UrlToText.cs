using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using System.Data;

namespace Sitecore.SharedSource.DataImporter.Mappings.Properties {
	public class UrlToText : BaseProperty {
		#region Properties

		#endregion Properties

		#region Constructor

		//constructor
		public UrlToText(Item i)
			: base(i) {
		}

		#endregion Constructor

		#region Methods

		//fills it's own field
		public override void FillField(ref Item newItem, Item importRow) {

			StringBuilder existingValue = new StringBuilder();
			string[] splitr = { "/" };
			string[] pathArr = importRow.Paths.Path.Split(splitr, StringSplitOptions.RemoveEmptyEntries);
			
			//removes /sitecore/content
			if (pathArr.Length > 3) {
				for (int i = 3; i < pathArr.Length; i++) {
					existingValue.Append("/" + pathArr[i]);
				}
			}

			existingValue.Append(".aspx");
			newItem.Fields[NewItemField].Value = existingValue.ToString();
		}

		#endregion Methods
	}
}
