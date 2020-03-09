using System;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	public class ToSingleItem : ToText
	{
		public ToSingleItem(Item i, ILogger l) : base(i, l)
		{
		}

		public override void FillField(IDataMap map, ref Item newItem, object importRow, string importValue)
		{
			Assert.IsNotNull(newItem, "newItem");
			if (string.IsNullOrEmpty(importValue))
				return;

			var firstVal = importValue.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

			//store the imported value as is
			Field f = newItem.Fields[NewItemField];
			if (f != null)
				f.Value = firstVal;
		}
	}
}