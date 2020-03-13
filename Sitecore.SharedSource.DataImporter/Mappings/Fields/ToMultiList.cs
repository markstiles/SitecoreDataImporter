using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{

	public class ToMultilist : ToText
	{
		#region properties

		public string SelectionRootItem { get; set; }

		#endregion properties

		public ToMultilist(Item i, ILogger l) : base(i, l)
		{
			SelectionRootItem = GetItemField(i, "SelectionRootItem");
			Delimiter = GetItemField(i, "Delimiter");
		}

		public override void FillField(IDataMap map, ref Item newItem, object importRow)
		{
			Assert.IsNotNull(newItem, "newItem");
			List<string> selectedList = new List<string>();
			if (string.IsNullOrEmpty(SelectionRootItem))
				return;
            
			Item root = newItem.Database.GetItem(SelectionRootItem);
			if (root == null)
				return;

			ChildList selectionValues = new ChildList(root);
            var importValue = string.Join(Delimiter, map.GetFieldValues(ExistingDataNames, importRow));
            if (string.IsNullOrEmpty(importValue) || !selectionValues.Any())
				return;

			List<string> importvalues = importValue.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim().ToLowerInvariant()).ToList();
			if (!importvalues.Any())
				return;

			selectedList.AddRange(selectionValues
		        .Where(v => importvalues.Contains(v.DisplayName.Trim().ToLowerInvariant()))
		        .Select(v => v.ID.ToString()));
            
			Field f = newItem.Fields[ToWhatField];
			if (f == null || !selectedList.Any())
				return;

			f.Value = string.Join("|", selectedList);
		}
	}
}