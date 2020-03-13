using System;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;
using System.Collections.Generic;

namespace Sitecore.SharedSource.DataImporter.Mappings.Fields.SitecoreFields
{
    public class ToImage : ToText
    {
        #region Properties

        protected MediaService MediaService { get; set; }
        
        #endregion Properties

        #region Constructor

        public ToImage(Item i, ILogger l) : base(i, l)
        {
            MediaService = new MediaService(l);
        }

        #endregion Constructor

        #region IBaseField

        public override void FillField(IDataMap map, ref Item newItem, object importRow)
        {
            var importItem = (Item)importRow;
            if (importItem == null)
                return;
            
            var mediaItem = MediaService.GetImage(importItem, ExistingDataNames);
            if (mediaItem == null)
                return;
            
            var newMedia = MediaService.FindOrCreateMediaItem(map, mediaItem);
            if (newMedia == null)
                return;

            ImageField f = newItem.Fields[ToWhatField];
            if (f == null)
                return;

            f.MediaID = newMedia.ID;            
        }

        #endregion IBaseField
    }
}
