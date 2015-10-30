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

        private string selectionRootItem;

        public string SelectionRootItem
        {
            get { return selectionRootItem; }
            set { selectionRootItem = value; }
        }

        #endregion properties

        public ToDropdownList(Item i) : base(i)
        {
            if (i.Fields["SelectionRootItem"] != null)
            {
                SelectionRootItem = i.Fields["SelectionRootItem"].Value;
            }
        }

        #region private methods

        public override void FillField(BaseDataMap map, ref Item newItem, string importValue)
        {
            string selectedValue = string.Empty;

            if (!string.IsNullOrEmpty(SelectionRootItem))
            {
                var master = Factory.GetDatabase("master");
                Item root = master.GetItem(selectionRootItem);
                if (root != null)
                {
                    ChildList selectionValues = new ChildList(root);
                    
                    if (!string.IsNullOrEmpty(importValue) && selectionValues.Any())
                    {
                        if (importValue.IsNotNull())
                        {
                            foreach (Item value in selectionValues)
                            {
                                if (value.Fields["Text"] != null)
                                {
                                    if (importValue.Trim().ToLower().Equals(value.Fields["Text"].ToString().Trim().ToLower()))
                                    {
                                        selectedValue = value.ID.ToString();
                                    }
                                }
                            }

                            Field f = newItem.Fields[NewItemField];
                            //store the imported value as is         
                            if (f != null && selectedValue.IsNotNull())
                            {
                                f.Value = selectedValue;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}