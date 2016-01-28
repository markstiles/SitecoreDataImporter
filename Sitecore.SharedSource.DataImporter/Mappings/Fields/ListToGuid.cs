using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Utility;
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
        public override void FillField(IDataMap map, ref Item newItem, string importValue)
        {

            if (string.IsNullOrEmpty(importValue))
                return;

            //get parent item of list to search
            Item i = newItem.Database.GetItem(SourceList);
            if (i == null)
                return;

            //loop through children and look for anything that matches by name
            string cleanName = StringUtility.GetValidItemName(importValue, map.ItemNameMaxLength);
            IEnumerable<Item> t = i.GetChildren().Where(c => c.DisplayName.Equals(cleanName));

            //if you find one then store the id
            if (!t.Any())
                return;

            Field f = newItem.Fields[NewItemField];
            if (f == null)
                return;

            f.Value = t.First().ID.ToString();
		}

        #endregion IBaseField
    }
}
