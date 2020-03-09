using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields
{
    public class SubitemFieldToComponent : FieldToComponent
    {
        #region Constructor

        public SubitemFieldToComponent(Item i, ILogger l) : base(i, l)
        {
            
        }

        #endregion Constructor

        #region IBaseField
        
        public override bool SetField(IDataMap map, Item datasource, object importRow, string importValue)
        {
            var importItem = (Item)importRow;
            var childItem = importItem.Axes.GetChild("Features and Benefits");
            if (childItem == null)
            {
                Logger.Log($"The Features and Benefits child item wasn't found", importItem.Paths.FullPath, ProcessStatus.FieldToComponentLog);
                return false;
            }

            var f = datasource.Fields[NewItemField];
            if (f == null)
            {
                Logger.Log($"The {NewItemField} is null", datasource.Paths.FullPath, ProcessStatus.FieldToComponentLog);
                return false;
            }

            datasource.Editing.BeginEdit();

            foreach (var field in ExistingDataNames)
            {
                var value = childItem.Fields[field]?.Value;
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                f.Value += MediaService.TransferImages((SitecoreDataMap)map, value);
            }
            
            datasource.Editing.EndEdit(false, false);

            datasource.Database.Caches.ItemCache.RemoveItem(datasource.ID);

            return true;
        }
        
        #endregion IBaseField
    }
}
