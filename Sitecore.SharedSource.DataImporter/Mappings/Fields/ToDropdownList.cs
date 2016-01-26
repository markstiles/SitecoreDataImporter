using System.Linq;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
 
    public class ToDropdownList : ToText, IBaseField
    {
        #region properties

        public string SelectionRootItem { get; set; }

        #endregion properties

        public ToDropdownList(Item i) : base(i)
        {
            SelectionRootItem = GetItemField(i, "SelectionRootItem");
        }

        #region private methods

        public override void FillField(IDataMap map, ref Item newItem, string importValue)
        {
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
            
            foreach (Item value in selectionValues) {
                Field t = value.Fields["Text"];
                if (t == null)
                    continue;

                if (!importValue.Trim().ToLower().Equals(t.Value.Trim().ToLower()))
                    continue;
                
                selectedValue = value.ID.ToString();
            }

            Field f = newItem.Fields[NewItemField];
            if (f == null || !selectedValue.IsNotNull())
                return;
            
            f.Value = selectedValue;
        }

        #endregion
    }
}