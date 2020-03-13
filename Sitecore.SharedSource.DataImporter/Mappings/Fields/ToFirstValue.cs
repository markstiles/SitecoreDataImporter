using System;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
	public class ToFirstValue : ToText
	{
		public ToFirstValue(Item i, ILogger l) : base(i, l)
		{
		}

		public override void FillField(IDataMap map, ref Item newItem, object importRow)
		{
			Assert.IsNotNull(newItem, "newItem");

            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));
            if (string.IsNullOrEmpty(importValue))
				return;

			var firstVal = importValue.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            Field f = newItem.Fields[ToWhatField];
			if (f != null)
				f.Value = firstVal;
		}
	}
}