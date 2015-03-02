using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DataImporter;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Data;
using Sitecore.SharedSource.DataImporter.Extensions;
using System.Collections;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Utility;
using Sitecore.Data.Fields;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {

    /// <summary>
    /// This uses imported values to match by name an existing content item in the list provided
    /// then stores the GUID of the existing item
    /// </summary>
    public class ListToGuid : ToText {

        #region Properties

        /// <summary>
        /// This is the list that you will compare the imported values against
        /// </summary>
        public string SourceList { get; set; }

        #endregion Properties

        #region Constructor

        public ListToGuid(Item i)
            : base(i) {
            //stores the source list value
            SourceList = GetItemField(i, "Source List");
        }

        #endregion Constructor

        #region IBaseField

        /// <summary>
        /// uses the import value to search for a matching item in the SourceList and then stores the GUID
        /// </summary>
        /// <param name="map">provides settings for the import</param>
        /// <param name="newItem">newly created item</param>
        /// <param name="importValue">imported value to match</param>
        public override void FillField(IDataMap map, ref Item newItem, string importValue) {
            //get parent item of list to search
            Item i = newItem.Database.GetItem(SourceList);
            if (i != null) {
                //loop through children and look for anything that matches by name
                IEnumerable<Item> t = from Item c in i.GetChildren()
                                      where c.DisplayName.Equals(StringUtility.GetNewItemName(importValue, map.ItemNameMaxLength))
                                      select c;
                //if you find one then store the id
                if (t.Any()) {
                    Field f = newItem.Fields[NewItemField];
                    if (f != null)
                        f.Value = t.First().ID.ToString();
                }
            }
        }

        #endregion IBaseField
    }
}
