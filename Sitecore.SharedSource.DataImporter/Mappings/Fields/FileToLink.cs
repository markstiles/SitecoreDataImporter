
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Data.Fields;
using Sitecore.SharedSource.DataImporter.Mappings.Properties;
using Sitecore.SharedSource.DataImporter.Utility;
using System.Net;
using System.IO;
using Sitecore.Resources.Media;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields {
    public class FileToLink : MediaFileMapping, IBaseFieldWithReference
    {
        #region Properties

        #endregion Properties

        #region Constructor

        //constructor
        public FileToLink(Item i, ILogger logger) : base(i, logger) { }

        #endregion Constructor

        #region IBaseProperty

        public string Name { get; set; }

        public void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldName)
        {
			Assert.IsNotNull(importRow, "importRow");
			Assert.IsNotNull(newItem, "newItem");

			importRow.Fields.ReadAll();
            ImageField oldField = importRow.Fields[fieldName]; 
            ReferenceField f = newItem.Fields[NewItemField];
            if (f != null)
                f.Value = ImportMediaItem(map, newItem, oldField).ToString();
        }        

        #endregion IBaseProperty

        private ID ImportMediaItem(IDataMap map, Item newItem, ImageField field)
        {
			if (field.MediaItem == null)
			{
				map.Logger.Log("FileToLink.ImportMediaItem", string.Format("Media field {0} was empty on item {1}", field.InnerField.Name, field.InnerField.Item.Paths.FullPath));
				return null;
			}

            var media = new MediaItem(field.MediaItem);
            var newMedia = HandleMediaItem(map, BuildMediaPath(newItem.Database, media.InnerItem.Paths.ParentPath), media.Path, media);

            return newMedia.ID;
        }


        public string GetExistingFieldName()
        {
            return GetItemField(InnerItem, "From What Fields");
        }
    }
}
