using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.Data.Fields;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Mappings.ReferenceFields
{
	public class ToMediaImage : BaseMapping, IBaseFieldWithReference
	{
		#region Properties

        protected MediaService MediaService { get; set; }

		#endregion Properties

		#region Constructor

		//constructor
		public ToMediaImage(Item i, ILogger l) : base(i, l)
        {
            MediaService = new MediaService(l);
        }

		#endregion Constructor

		#region IBaseProperty

		public string Name { get; set; }

		public virtual void FillField(IDataMap map, ref Item newItem, Item importRow, string fieldName)
		{
			Assert.IsNotNull(importRow, "importRow");
			Assert.IsNotNull(newItem, "newItem");

			importRow.Fields.ReadAll();
			ImageField oldField = importRow.Fields[fieldName];
			ImageField f = newItem.Fields[NewItemField];

			if (f == null) return;

			if (oldField?.MediaItem != null)
			{
				f.MediaID = ImportMediaItem(map, newItem, oldField.MediaItem);
			}
			else
			{
				f.Value = string.Empty;
			}
		}

		#endregion IBaseProperty

		protected ID ImportMediaItem(IDataMap map, Item newItem, Item mediaItem)
		{
			var media = new MediaItem(mediaItem);
			var newMedia = MediaService.FindOrCreateMediaItem(map, media);

			return newMedia.ID;
		}

		public string GetExistingFieldName()
		{
			return GetItemField(InnerItem, "From What Fields");
		}
	}
}
