using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Services;
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

        public StringService StringService { get; set; }

        #endregion Properties

        #region Constructor

        public ListToGuid(Item i, ILogger l) : base(i, l) {
            //stores the source list value
            SourceList = GetItemField(i, "Source List");
            StringService = new StringService();
        }

        #endregion Constructor

        #region IBaseField

        /// <summary>
        /// uses the import value to search for a matching item in the SourceList and then stores the GUID
        /// </summary>
        /// <param name="map">provides settings for the import</param>
        /// <param name="newItem">newly created item</param>
        /// <param name="importValue">imported value to match</param>
        public override void FillField(IDataMap map, ref Item newItem, object importRow, string importValue)
        {
            if (string.IsNullOrEmpty(importValue))
                return;

            Item i = InnerItem.Database.GetItem(SourceList);
            if (i == null)
                return;

            var importItem = importRow is Item ? (Item)importRow : null;
            
            string cleanName = ID.IsID(importValue)
                ? importItem?.Database?.GetItem(new ID(importValue))?.DisplayName
                : StringService.GetValidItemName(importValue, map.ItemNameMaxLength);

            if (string.IsNullOrWhiteSpace(cleanName))
                return;

            IEnumerable<Item> t = i.Axes.GetDescendants().Where(c => c.DisplayName.Equals(cleanName));
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
