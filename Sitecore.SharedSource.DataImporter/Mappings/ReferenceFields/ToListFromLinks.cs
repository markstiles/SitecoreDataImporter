using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.ReferenceFields
{
	public class ToListFromLinks : BaseMapping, IBaseFieldWithReference
	{
		public ToListFromLinks(Item i, ILogger l) : base(i, l)
		{

		}

		public string Name { get; set; }
		public void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldName)
		{
			Assert.IsNotNull(importRow, "importRow");
			Assert.IsNotNull(newItem, "newItem");

			var fields = fieldName.Split(',');
			var values = new List<string>();

			foreach (var field in fields)
			{
				LinkField f = importRow.Fields[field];

				if (f == null) continue;

				if (f.IsInternal && (f.TargetItem?.Paths.IsContentItem ?? false))
				{
					values.Add(f.TargetID.ToString());
				}
			}

			if (!values.Any()) return;

			newItem.Fields[NewItemField].Value = string.Join("|", values);
		}

		public string GetExistingFieldName()
		{
			return GetItemField(InnerItem, "From What Fields");
		}
	}
}