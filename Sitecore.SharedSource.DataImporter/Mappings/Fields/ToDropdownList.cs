using System.Linq;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
 
    public class ToDropdownList : ToText, IBaseField
    {
        #region properties

        public string SelectionRootItem { get; set; }

        #endregion properties

        public ToDropdownList(Item i, ILogger l) : base(i, l)
        {
            SelectionRootItem = GetItemField(i, "SelectionRootItem");
        }

        #region private methods

        public override void FillField(IDataMap map, ref Item newItem, object importRow, string importValue)
        {
			Assert.IsNotNull(newItem, "newItem");
            string selectedValue = string.Empty;

            if (string.IsNullOrEmpty(SelectionRootItem))
                return;
            
            var master = Factory.GetDatabase("master");
            Item root = master.GetItem(SelectionRootItem);
            if (root == null)
                return;
            
            ChildList selectionValues = new ChildList(root);
            if (string.IsNullOrEmpty(importValue) || !selectionValues.Any())
                return;

            if (!importValue.IsNotNull())
                return;

	        importValue = importValue.Trim().ToLowerInvariant();
	        selectedValue = selectionValues.FirstOrDefault(v => v.Fields["Text"] != null && v.Fields["Text"].Value.Trim().ToLowerInvariant() == importValue)?.ID.ToString();
            

            Field f = newItem.Fields[NewItemField];
            if (f == null || !selectedValue.IsNotNull())
                return;
            
            f.Value = selectedValue;
        }

        #endregion
    }
}