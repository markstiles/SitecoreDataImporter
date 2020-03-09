using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.ReferenceFields
{
	public class ToMediaLink : ToMediaImage, IBaseFieldWithReference
	{
		public ToMediaLink(Item i, ILogger l) : base(i, l)
		{
		}

		public override void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldNames)
		{
			Assert.IsNotNull(importRow, "importRow");
			Assert.IsNotNull(newItem, "newItem");

			var fields = fieldNames.Split(',');
			string fieldName = fields[0];
			string textFieldName = fields.Length == 2 ? fields[1] : string.Empty;

			importRow.Fields.ReadAll();
			LinkField oldField = importRow.Fields[fieldName];
			LinkField f = newItem.Fields[NewItemField];
			if (f != null && oldField != null)
			{
				f.Value = oldField.Value;
				if (!string.IsNullOrEmpty(textFieldName))
				{
					string textValue = importRow.Fields[textFieldName]?.Value;
					if (!string.IsNullOrEmpty(textValue))
					{
						f.Text = textValue;
					}
				}

				if (oldField.IsMediaLink && oldField.TargetItem != null)
				{
					f.TargetID = ImportMediaItem(map, newItem, oldField.TargetItem);
				}
			}	
		}
	}
}