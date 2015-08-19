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
            private string selectionRootItem;

            public string SelectionRootItem
            {
                get { return selectionRootItem; }
                set { selectionRootItem = value; }
            }

        #endregion properties

        public ToListItem(Item i)  : base(i)
        {
            if (i.Fields["SelectionRootItem"] != null)
            {
                SelectionRootItem = i.Fields["SelectionRootItem"].Value;
            }
            if (i.Fields["Delimiter"] != null)
            {
                Delimiter = i.Fields["Delimiter"].Value;
            }
        }

        public override void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            List<string> selectedList = new List<string>();
            if (!string.IsNullOrEmpty(SelectionRootItem))
            {
                var master = Factory.GetDatabase("master");
                Item root = master.GetItem(selectionRootItem);
                if (root != null)
                {
                    ChildList selectionValues = new ChildList(root);

                    if (!string.IsNullOrEmpty(importValue) && selectionValues.Any())
                    {
                        List<string> importvalues = importValue.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (importvalues.Any())
                        {
                            foreach (Item value in selectionValues)
                            {
                                foreach (var temp in importvalues)
                                {
                                    if (value.Fields["Text"] != null)
                                    { 
                                        if (temp.Trim().ToLower().Equals(value.Fields["Text"].ToString().Trim().ToLower()))
                                        {
                                            selectedList.Add(value.ID.ToString());
                                        }
                                    }
                                }
                            }

                            Field f = newItem.Fields[NewItemField];
                            //store the imported value as is         
                            if (f != null && selectedList.Any())
                            {
                                f.Value = string.Join("|", selectedList);
                            }
                        }
                    }
                }
            }
        }
    }
}