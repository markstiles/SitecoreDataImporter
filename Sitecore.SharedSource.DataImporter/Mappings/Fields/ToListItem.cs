using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{

    public class ToListItem : ToText, IBaseField
    {
        #region properties
            
        public string SelectionRootItem { get; set; }

        #endregion properties

        public ToListItem(Item i)  : base(i)
        {
            SelectionRootItem = GetItemField(i, "SelectionRootItem");
            Delimiter = GetItemField(i, "Delimiter");
        }

        public override void FillField(IDataMap map, ref Item newItem, string importValue)
        {
            List<string> selectedList = new List<string>();
            if (string.IsNullOrEmpty(SelectionRootItem))
                return;
            
            var master = Factory.GetDatabase("master");
            Item root = master.GetItem(SelectionRootItem);
            if (root == null)
                return;
            
            ChildList selectionValues = new ChildList(root);
            if (string.IsNullOrEmpty(importValue) || !selectionValues.Any())
                return;
            
            List<string> importvalues = importValue.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!importvalues.Any())
                return;
            
            foreach (Item value in selectionValues) {
                foreach (var temp in importvalues) {
                    Field t = value.Fields["Text"];
                    if (t == null)
                        continue;
                        
                    if (!temp.Trim().ToLower().Equals(t.Value.Trim().ToLower()))
                        continue;

                    selectedList.Add(value.ID.ToString());
                }
            }

            Field f = newItem.Fields[NewItemField];
            if (f == null || !selectedList.Any())
                return;

            f.Value = string.Join("|", selectedList);
        }
    }
}