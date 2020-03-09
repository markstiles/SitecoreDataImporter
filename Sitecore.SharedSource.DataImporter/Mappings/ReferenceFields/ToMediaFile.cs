using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.ReferenceFields
{
	public class ToMediaFile : ToMediaImage, IBaseFieldWithReference
	{
		public ToMediaFile(Item i, ILogger l) : base(i, l)
		{
		}

		public override void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldNames)
		{
			Assert.IsNotNull(importRow, "importRow");
			Assert.IsNotNull(newItem, "newItem");

			var fields = fieldNames.Split(',');
			string fieldName = fields[0];

			importRow.Fields.ReadAll();
			FileField oldField = importRow.Fields[fieldName];
			FileField f = newItem.Fields[NewItemField];
			if (f != null && oldField != null)
			{
				f.Value = oldField.Value;

				if (oldField.MediaItem != null)
				{
					f.MediaID = ImportMediaItem(map, newItem, oldField.MediaItem);
				}
			}
		}
	}
}