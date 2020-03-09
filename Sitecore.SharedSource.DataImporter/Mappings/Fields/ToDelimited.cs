using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{

	public class ToDelimited : ToText, IBaseField
	{
		#region properties

		public string SelectionRootItem { get; set; }

        public StringService StringService { get; set; }

        #endregion properties

        public ToDelimited(Item i, ILogger l) : base(i, l)
		{
			SelectionRootItem = GetItemField(i, "SelectionRootItem");
			Delimiter = GetItemField(i, "Delimiter");
            StringService = new StringService();
        }

		public override void FillField(IDataMap map, ref Item newItem, object importRow, string importValue)
		{
			Assert.IsNotNull(newItem, "newItem");
			List<string> selectedList = new List<string>();
			if (string.IsNullOrEmpty(SelectionRootItem))
				return;

			var master = Factory.GetDatabase("master");
			Item root = master.GetItem(SelectionRootItem);
			if (root == null)
				return;

			var selectionValues = root.Axes.GetDescendants();
			if (string.IsNullOrEmpty(importValue))
				return;

			List<string> importvalues = importValue.Split(new[] { Delimiter.Trim(), $" {Delimiter} ", $"{Delimiter} ", $" {Delimiter}" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(i => i.Trim())
				.ToList();
			if (!importvalues.Any())
				return;

			Field f = newItem.Fields[NewItemField];
			if (f == null) return;

			foreach (var value in importvalues)
			{
				selectionValues = root.Axes.GetDescendants();
				foreach (var option in selectionValues)
				{
					var optionName = option.DisplayName.Trim().ToLowerInvariant();
					var valueName = StringService.GetValidItemName(value.ToLowerInvariant(), map.ItemNameMaxLength).Trim(); 
					// Match items in taxonomy folder and avoid duplicates added to the list 
					if (valueName == optionName)
					{
						selectedList.Add(option.ID.ToString());
					}
				}
			}

			if (!selectedList.Any())
				return;

			f.Value = string.Join("|", selectedList);
		}
	}
}